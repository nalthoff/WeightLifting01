using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutLabel;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts.UpdateWorkoutLabel;

public sealed class UpdateWorkoutLabelCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncUpdatesLabelForInProgressWorkout()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = await SeedWorkoutAsync(dbContext, WorkoutStatus.InProgress, "Before");
        var handler = new UpdateWorkoutLabelCommandHandler(dbContext);

        var result = await handler.HandleAsync(new UpdateWorkoutLabelCommand
        {
            WorkoutId = workoutId,
            Label = "  After  ",
        }, CancellationToken.None);

        var entity = await dbContext.Workouts.SingleAsync(workout => workout.Id == workoutId);
        Assert.Equal(UpdateWorkoutLabelOutcome.Updated, result.Outcome);
        Assert.Equal("After", entity.Label);
        Assert.Equal("After", result.Workout?.Label);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsyncTreatsBlankOrWhitespaceAsUnnamed(string input)
    {
        await using var dbContext = CreateDbContext();
        var workoutId = await SeedWorkoutAsync(dbContext, WorkoutStatus.InProgress, "Named");
        var handler = new UpdateWorkoutLabelCommandHandler(dbContext);

        var result = await handler.HandleAsync(new UpdateWorkoutLabelCommand
        {
            WorkoutId = workoutId,
            Label = input,
        }, CancellationToken.None);

        var entity = await dbContext.Workouts.SingleAsync(workout => workout.Id == workoutId);
        Assert.Equal(UpdateWorkoutLabelOutcome.Updated, result.Outcome);
        Assert.Null(entity.Label);
        Assert.Null(result.Workout?.Label);
    }

    [Fact]
    public async Task HandleAsyncReturnsConflictForCompletedWorkout()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = await SeedWorkoutAsync(
            dbContext,
            WorkoutStatus.Completed,
            "Done",
            completedAtUtc: new DateTime(2026, 4, 24, 12, 0, 0, DateTimeKind.Utc));
        var handler = new UpdateWorkoutLabelCommandHandler(dbContext);

        var result = await handler.HandleAsync(new UpdateWorkoutLabelCommand
        {
            WorkoutId = workoutId,
            Label = "Should Fail",
        }, CancellationToken.None);

        Assert.Equal(UpdateWorkoutLabelOutcome.Conflict, result.Outcome);
        Assert.Contains("Workout must be in progress to edit name.", result.Errors["workout"]);
    }

    [Fact]
    public async Task HandleAsyncReturnsValidationFailedForTooLongLabel()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = await SeedWorkoutAsync(dbContext, WorkoutStatus.InProgress, "Named");
        var handler = new UpdateWorkoutLabelCommandHandler(dbContext);

        var result = await handler.HandleAsync(new UpdateWorkoutLabelCommand
        {
            WorkoutId = workoutId,
            Label = new string('x', Workout.MaxLabelLength + 1),
        }, CancellationToken.None);

        Assert.Equal(UpdateWorkoutLabelOutcome.ValidationFailed, result.Outcome);
        Assert.Contains($"Workout label must be {Workout.MaxLabelLength} characters or fewer.", result.Errors["label"]);
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }

    private static async Task<Guid> SeedWorkoutAsync(
        WeightLiftingDbContext dbContext,
        WorkoutStatus status,
        string? label,
        DateTime? completedAtUtc = null)
    {
        var workoutId = Guid.NewGuid();
        var startedAtUtc = new DateTime(2026, 4, 24, 10, 0, 0, DateTimeKind.Utc);
        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workoutId,
            UserId = "default-user",
            Status = status,
            Label = label,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc,
            CreatedAtUtc = startedAtUtc,
            UpdatedAtUtc = completedAtUtc ?? startedAtUtc,
        });
        await dbContext.SaveChangesAsync();
        return workoutId;
    }
}
