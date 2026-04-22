using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.CompleteWorkout;
using WeightLifting.Api.Application.Workouts.Queries.GetInProgressWorkout;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class CompleteWorkoutIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task CompleteWorkoutMarksWorkoutCompletedAndRemovesActiveSummary()
    {
        var workoutId = Guid.NewGuid();
        var startedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);
        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workoutId,
            UserId = "default-user",
            Status = WorkoutStatus.InProgress,
            Label = "Session",
            StartedAtUtc = startedAtUtc,
            CreatedAtUtc = startedAtUtc,
            UpdatedAtUtc = startedAtUtc,
        });
        await dbContext.SaveChangesAsync();

        var commandHandler = new CompleteWorkoutCommandHandler(dbContext);
        var inProgressHelper = new GetInProgressWorkoutQueryHelper(dbContext);

        var result = await commandHandler.HandleAsync(new CompleteWorkoutCommand
        {
            WorkoutId = workoutId,
        }, CancellationToken.None);
        var activeWorkout = await inProgressHelper.GetAsync("default-user", CancellationToken.None);
        var persistedWorkout = await dbContext.Workouts.SingleAsync(workout => workout.Id == workoutId);

        Assert.Equal(CompleteWorkoutOutcome.Completed, result.Outcome);
        Assert.Null(activeWorkout);
        Assert.Equal(WorkoutStatus.Completed, persistedWorkout.Status);
        Assert.NotNull(persistedWorkout.CompletedAtUtc);
    }

    [Fact]
    public async Task CompleteWorkoutWhenMissingReturnsNotFound()
    {
        var handler = new CompleteWorkoutCommandHandler(dbContext);

        var result = await handler.HandleAsync(new CompleteWorkoutCommand
        {
            WorkoutId = Guid.NewGuid(),
        }, CancellationToken.None);

        Assert.Equal(CompleteWorkoutOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task CompleteWorkoutWhenAlreadyCompletedReturnsConflict()
    {
        var workoutId = Guid.NewGuid();
        var startedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);
        var completedAtUtc = startedAtUtc.AddMinutes(40);
        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workoutId,
            UserId = "default-user",
            Status = WorkoutStatus.Completed,
            Label = "Session",
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc,
            CreatedAtUtc = startedAtUtc,
            UpdatedAtUtc = completedAtUtc,
        });
        await dbContext.SaveChangesAsync();

        var handler = new CompleteWorkoutCommandHandler(dbContext);
        var result = await handler.HandleAsync(new CompleteWorkoutCommand
        {
            WorkoutId = workoutId,
        }, CancellationToken.None);

        Assert.Equal(CompleteWorkoutOutcome.Conflict, result.Outcome);
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
}
