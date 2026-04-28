using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.CompleteWorkout;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts.CompleteWorkout;

public sealed class CompleteWorkoutCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncCompletesInProgressWorkout()
    {
        await using var dbContext = CreateDbContext();
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

        var handler = new CompleteWorkoutCommandHandler(dbContext);
        var result = await handler.HandleAsync(new CompleteWorkoutCommand
        {
            WorkoutId = workoutId,
        }, CancellationToken.None);

        var persistedWorkout = await dbContext.Workouts.SingleAsync(workout => workout.Id == workoutId);

        Assert.Equal(CompleteWorkoutOutcome.Completed, result.Outcome);
        Assert.NotNull(result.Workout);
        Assert.Equal(WorkoutStatus.Completed, result.Workout.Status);
        Assert.NotNull(result.Workout.CompletedAtUtc);
        Assert.Equal(WorkoutStatus.Completed, persistedWorkout.Status);
        Assert.NotNull(persistedWorkout.CompletedAtUtc);
        Assert.Equal(persistedWorkout.CompletedAtUtc, persistedWorkout.UpdatedAtUtc);
    }

    [Fact]
    public async Task HandleAsyncWhenWorkoutMissingReturnsNotFound()
    {
        await using var dbContext = CreateDbContext();
        var handler = new CompleteWorkoutCommandHandler(dbContext);

        var result = await handler.HandleAsync(new CompleteWorkoutCommand
        {
            WorkoutId = Guid.NewGuid(),
        }, CancellationToken.None);

        Assert.Equal(CompleteWorkoutOutcome.NotFound, result.Outcome);
        Assert.Null(result.Workout);
    }

    [Fact]
    public async Task HandleAsyncWhenWorkoutNotInProgressReturnsConflict()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var startedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);

        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workoutId,
            UserId = "default-user",
            Status = WorkoutStatus.Completed,
            Label = "Session",
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = startedAtUtc.AddMinutes(30),
            CreatedAtUtc = startedAtUtc,
            UpdatedAtUtc = startedAtUtc.AddMinutes(30),
        });
        await dbContext.SaveChangesAsync();

        var handler = new CompleteWorkoutCommandHandler(dbContext);
        var result = await handler.HandleAsync(new CompleteWorkoutCommand
        {
            WorkoutId = workoutId,
        }, CancellationToken.None);

        Assert.Equal(CompleteWorkoutOutcome.Conflict, result.Outcome);
        Assert.Null(result.Workout);
        Assert.Contains("Workout must be in progress to complete.", result.Errors["workout"]);
    }

    [Fact]
    public async Task HandleAsyncWhenWorkoutAlreadyCompletedDoesNotMutateCompletionTimestamp()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var startedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);
        var completedAtUtc = startedAtUtc.AddMinutes(30);

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
        var persistedWorkout = await dbContext.Workouts.SingleAsync(workout => workout.Id == workoutId);

        Assert.Equal(CompleteWorkoutOutcome.Conflict, result.Outcome);
        Assert.Null(result.Workout);
        Assert.Contains("Workout must be in progress to complete.", result.Errors["workout"]);
        Assert.Equal(completedAtUtc, persistedWorkout.CompletedAtUtc);
        Assert.Equal(completedAtUtc, persistedWorkout.UpdatedAtUtc);
    }

    [Fact]
    public async Task HandleAsyncWhenAnotherWorkoutIsActiveCompletesHistoricalWorkflowAndPreservesActiveWorkout()
    {
        await using var dbContext = CreateDbContext();
        var activeWorkoutId = Guid.NewGuid();
        var historicalWorkoutId = Guid.NewGuid();
        var startedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);

        dbContext.Workouts.AddRange(
            new WorkoutEntity
            {
                Id = activeWorkoutId,
                UserId = "default-user",
                Status = WorkoutStatus.InProgress,
                Label = "Current Session",
                StartedAtUtc = startedAtUtc,
                CreatedAtUtc = startedAtUtc,
                UpdatedAtUtc = startedAtUtc,
            },
            new WorkoutEntity
            {
                Id = historicalWorkoutId,
                UserId = "default-user",
                Status = WorkoutStatus.InProgress,
                Label = "Backfill Draft",
                StartedAtUtc = startedAtUtc.AddDays(-2),
                CreatedAtUtc = startedAtUtc.AddDays(-2),
                UpdatedAtUtc = startedAtUtc.AddDays(-2),
            });
        await dbContext.SaveChangesAsync();

        var handler = new CompleteWorkoutCommandHandler(dbContext);
        var result = await handler.HandleAsync(new CompleteWorkoutCommand
        {
            WorkoutId = historicalWorkoutId,
        }, CancellationToken.None);

        var activeWorkout = await dbContext.Workouts.SingleAsync(workout => workout.Id == activeWorkoutId);
        var completedHistoricalWorkout = await dbContext.Workouts.SingleAsync(workout => workout.Id == historicalWorkoutId);

        Assert.Equal(CompleteWorkoutOutcome.Completed, result.Outcome);
        Assert.Null(activeWorkout.CompletedAtUtc);
        Assert.Equal(WorkoutStatus.InProgress, activeWorkout.Status);
        Assert.Equal(WorkoutStatus.Completed, completedHistoricalWorkout.Status);
        Assert.NotNull(completedHistoricalWorkout.CompletedAtUtc);
    }

    [Fact]
    public async Task HandleAsyncWhenCompletionTimestampProvidedUsesExplicitTimestamp()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var startedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);
        var explicitCompletedAtUtc = new DateTime(2026, 4, 22, 12, 42, 0, DateTimeKind.Utc);

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

        var handler = new CompleteWorkoutCommandHandler(dbContext);
        var result = await handler.HandleAsync(new CompleteWorkoutCommand
        {
            WorkoutId = workoutId,
            CompletedAtUtc = explicitCompletedAtUtc,
        }, CancellationToken.None);
        var persistedWorkout = await dbContext.Workouts.SingleAsync(workout => workout.Id == workoutId);

        Assert.Equal(CompleteWorkoutOutcome.Completed, result.Outcome);
        Assert.NotNull(result.Workout);
        Assert.Equal(explicitCompletedAtUtc, result.Workout.CompletedAtUtc);
        Assert.Equal(explicitCompletedAtUtc, persistedWorkout.CompletedAtUtc);
        Assert.Equal(explicitCompletedAtUtc, persistedWorkout.UpdatedAtUtc);
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }
}
