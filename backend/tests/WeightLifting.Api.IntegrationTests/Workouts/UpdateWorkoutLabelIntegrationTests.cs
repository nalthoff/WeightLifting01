using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.CompleteWorkout;
using WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutLabel;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class UpdateWorkoutLabelIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task UpdateThenCompleteKeepsLatestNameAndAllowsCompletion()
    {
        var workoutId = await SeedInProgressWorkoutAsync("Initial");
        var renameHandler = new UpdateWorkoutLabelCommandHandler(dbContext);
        var completeHandler = new CompleteWorkoutCommandHandler(dbContext);

        var renameResult = await renameHandler.HandleAsync(new UpdateWorkoutLabelCommand
        {
            WorkoutId = workoutId,
            Label = "  Updated Name  ",
        }, CancellationToken.None);

        var completeResult = await completeHandler.HandleAsync(new CompleteWorkoutCommand
        {
            WorkoutId = workoutId,
        }, CancellationToken.None);

        Assert.Equal(UpdateWorkoutLabelOutcome.Updated, renameResult.Outcome);
        Assert.Equal(CompleteWorkoutOutcome.Completed, completeResult.Outcome);
        Assert.Equal("Updated Name", completeResult.Workout?.Label);
    }

    [Fact]
    public async Task UpdateWithWhitespaceClearsLabel()
    {
        var workoutId = await SeedInProgressWorkoutAsync("To Clear");
        var handler = new UpdateWorkoutLabelCommandHandler(dbContext);

        var result = await handler.HandleAsync(new UpdateWorkoutLabelCommand
        {
            WorkoutId = workoutId,
            Label = "   ",
        }, CancellationToken.None);

        var entity = await dbContext.Workouts.SingleAsync(workout => workout.Id == workoutId);
        Assert.Equal(UpdateWorkoutLabelOutcome.Updated, result.Outcome);
        Assert.Null(entity.Label);
    }

    [Fact]
    public async Task UpdateForCompletedWorkoutReturnsConflict()
    {
        var workoutId = await SeedInProgressWorkoutAsync("Completed");
        var completeHandler = new CompleteWorkoutCommandHandler(dbContext);
        await completeHandler.HandleAsync(new CompleteWorkoutCommand
        {
            WorkoutId = workoutId,
        }, CancellationToken.None);

        var renameHandler = new UpdateWorkoutLabelCommandHandler(dbContext);
        var result = await renameHandler.HandleAsync(new UpdateWorkoutLabelCommand
        {
            WorkoutId = workoutId,
            Label = "Nope",
        }, CancellationToken.None);

        Assert.Equal(UpdateWorkoutLabelOutcome.Conflict, result.Outcome);
    }

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseSqlite(connection)
            .Options;

        dbContext = new WeightLiftingDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await dbContext.DisposeAsync();
        await connection.DisposeAsync();
    }

    private async Task<Guid> SeedInProgressWorkoutAsync(string? label)
    {
        var startedAtUtc = new DateTime(2026, 4, 24, 12, 0, 0, DateTimeKind.Utc);
        var entity = new WorkoutEntity
        {
            Id = Guid.NewGuid(),
            UserId = "default-user",
            Status = WorkoutStatus.InProgress,
            Label = label,
            StartedAtUtc = startedAtUtc,
            CreatedAtUtc = startedAtUtc,
            UpdatedAtUtc = startedAtUtc,
        };
        dbContext.Workouts.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.Id;
    }
}
