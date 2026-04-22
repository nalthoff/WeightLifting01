using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.StartWorkout;
using WeightLifting.Api.Application.Workouts.Queries.GetInProgressWorkout;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts.StartWorkout;

public sealed class StartWorkoutCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncCreatesInProgressWorkoutWhenNoneExists()
    {
        await using var dbContext = CreateDbContext();
        var queryHelper = new GetInProgressWorkoutQueryHelper(dbContext);
        var handler = new StartWorkoutCommandHandler(dbContext, queryHelper);

        var result = await handler.HandleAsync(new StartWorkoutCommand
        {
            Label = "  Leg Day  ",
        }, CancellationToken.None);

        var persistedWorkout = await dbContext.Workouts.SingleAsync();

        Assert.Equal(StartWorkoutOutcome.Created, result.Outcome);
        Assert.Equal(WorkoutStatus.InProgress, result.Workout.Status);
        Assert.Equal("default-user", result.Workout.UserId);
        Assert.Equal("Leg Day", result.Workout.Label);
        Assert.Equal(result.Workout.Id, persistedWorkout.Id);
        Assert.Equal("default-user", persistedWorkout.UserId);
        Assert.Equal(WorkoutStatus.InProgress, persistedWorkout.Status);
        Assert.Equal("Leg Day", persistedWorkout.Label);
    }

    [Fact]
    public async Task HandleAsyncReturnsExistingWorkoutWhenOneIsAlreadyInProgress()
    {
        await using var dbContext = CreateDbContext();
        var existingWorkoutId = Guid.NewGuid();
        var existingStartedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);
        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = existingWorkoutId,
            UserId = "default-user",
            Status = WorkoutStatus.InProgress,
            Label = "Existing Session",
            StartedAtUtc = existingStartedAtUtc,
            CreatedAtUtc = existingStartedAtUtc,
            UpdatedAtUtc = existingStartedAtUtc,
        });
        await dbContext.SaveChangesAsync();

        var queryHelper = new GetInProgressWorkoutQueryHelper(dbContext);
        var handler = new StartWorkoutCommandHandler(dbContext, queryHelper);

        var result = await handler.HandleAsync(new StartWorkoutCommand
        {
            Label = "New Attempt",
        }, CancellationToken.None);

        Assert.Equal(StartWorkoutOutcome.AlreadyInProgress, result.Outcome);
        Assert.Equal(existingWorkoutId, result.Workout.Id);
        Assert.Equal("Existing Session", result.Workout.Label);
        Assert.Equal(1, await dbContext.Workouts.CountAsync());
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData("  Tempo  ", "Tempo")]
    public async Task HandleAsyncNormalizesOptionalLabel(string? inputLabel, string? expectedStoredLabel)
    {
        await using var dbContext = CreateDbContext();
        var queryHelper = new GetInProgressWorkoutQueryHelper(dbContext);
        var handler = new StartWorkoutCommandHandler(dbContext, queryHelper);

        var result = await handler.HandleAsync(new StartWorkoutCommand
        {
            Label = inputLabel,
        }, CancellationToken.None);

        var persistedWorkout = await dbContext.Workouts.SingleAsync(workout => workout.Id == result.Workout.Id);

        Assert.Equal(expectedStoredLabel, result.Workout.Label);
        Assert.Equal(expectedStoredLabel, persistedWorkout.Label);
    }

    [Fact]
    public void NormalizeLabelReturnsNullForNullOrWhitespaceAndTrimsNonEmpty()
    {
        Assert.Null(Workout.NormalizeLabel(null));
        Assert.Null(Workout.NormalizeLabel("   "));
        Assert.Equal("AM Session", Workout.NormalizeLabel("  AM Session  "));
    }

    [Fact]
    public void NormalizeLabelThrowsWhenLabelExceedsMaxLength()
    {
        var tooLongLabel = new string('x', Workout.MaxLabelLength + 1);

        var action = () => Workout.NormalizeLabel(tooLongLabel);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("label", exception.ParamName);
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }
}
