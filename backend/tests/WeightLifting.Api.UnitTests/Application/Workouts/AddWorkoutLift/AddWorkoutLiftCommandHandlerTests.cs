using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.AddWorkoutLift;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts.AddWorkoutLift;

public sealed class AddWorkoutLiftCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncAddsWorkoutLiftEntryForInProgressWorkoutAndActiveLift()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var liftId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.InProgress);
        SeedLift(dbContext, liftId, "Front Squat", isActive: true);
        await dbContext.SaveChangesAsync();

        var handler = new AddWorkoutLiftCommandHandler(dbContext);

        var beforeUtc = DateTime.UtcNow;
        var result = await handler.HandleAsync(new AddWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            LiftId = liftId,
        }, CancellationToken.None);
        var afterUtc = DateTime.UtcNow;

        var persistedEntry = await dbContext.WorkoutLiftEntries.SingleAsync();

        Assert.Equal(workoutId, result.WorkoutLift.WorkoutId);
        Assert.Equal(liftId, result.WorkoutLift.LiftId);
        Assert.Equal("Front Squat", result.WorkoutLift.DisplayName);
        Assert.Equal(1, result.WorkoutLift.Position);
        Assert.InRange(result.WorkoutLift.AddedAtUtc, beforeUtc, afterUtc);

        Assert.Equal(result.WorkoutLift.Id, persistedEntry.Id);
        Assert.Equal("Front Squat", persistedEntry.DisplayName);
        Assert.Equal(1, persistedEntry.Position);
    }

    [Fact]
    public async Task HandleAsyncAllowsDuplicateLiftEntriesAndAssignsNextPosition()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var liftId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.InProgress);
        SeedLift(dbContext, liftId, "Bench Press", isActive: true);
        await dbContext.SaveChangesAsync();

        var handler = new AddWorkoutLiftCommandHandler(dbContext);

        var first = await handler.HandleAsync(new AddWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            LiftId = liftId,
        }, CancellationToken.None);

        var second = await handler.HandleAsync(new AddWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            LiftId = liftId,
        }, CancellationToken.None);

        var entries = await dbContext.WorkoutLiftEntries
            .OrderBy(entry => entry.Position)
            .ToListAsync();

        Assert.Equal(1, first.WorkoutLift.Position);
        Assert.Equal(2, second.WorkoutLift.Position);
        Assert.Equal(2, entries.Count);
        Assert.All(entries, entry => Assert.Equal(liftId, entry.LiftId));
    }

    [Fact]
    public async Task HandleAsyncThrowsWhenWorkoutIsNotInProgress()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var liftId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.Completed);
        SeedLift(dbContext, liftId, "Deadlift", isActive: true);
        await dbContext.SaveChangesAsync();

        var handler = new AddWorkoutLiftCommandHandler(dbContext);

        var action = () => handler.HandleAsync(new AddWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            LiftId = liftId,
        }, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<WorkoutNotInProgressException>(action);
        Assert.Equal(workoutId, exception.WorkoutId);
    }

    [Fact]
    public async Task HandleAsyncThrowsWhenLiftIsInactive()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var liftId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.InProgress);
        SeedLift(dbContext, liftId, "Row", isActive: false);
        await dbContext.SaveChangesAsync();

        var handler = new AddWorkoutLiftCommandHandler(dbContext);

        var action = () => handler.HandleAsync(new AddWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            LiftId = liftId,
        }, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<LiftNotActiveException>(action);
        Assert.Equal(liftId, exception.LiftId);
    }

    [Fact]
    public async Task HandleAsyncThrowsWhenWorkoutOrLiftIsMissing()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var liftId = Guid.NewGuid();
        var existingWorkoutId = Guid.NewGuid();
        var existingLiftId = Guid.NewGuid();
        SeedWorkout(dbContext, existingWorkoutId, WorkoutStatus.InProgress);
        SeedLift(dbContext, existingLiftId, "Incline Bench", isActive: true);
        await dbContext.SaveChangesAsync();

        var handler = new AddWorkoutLiftCommandHandler(dbContext);

        var missingWorkoutAction = () => handler.HandleAsync(new AddWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            LiftId = existingLiftId,
        }, CancellationToken.None);

        var missingLiftAction = () => handler.HandleAsync(new AddWorkoutLiftCommand
        {
            WorkoutId = existingWorkoutId,
            LiftId = liftId,
        }, CancellationToken.None);

        var missingWorkoutException = await Assert.ThrowsAsync<KeyNotFoundException>(missingWorkoutAction);
        var missingLiftException = await Assert.ThrowsAsync<KeyNotFoundException>(missingLiftAction);
        Assert.Equal("Workout was not found.", missingWorkoutException.Message);
        Assert.Equal("Lift was not found.", missingLiftException.Message);
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }

    private static void SeedWorkout(
        WeightLiftingDbContext dbContext,
        Guid workoutId,
        WorkoutStatus status)
    {
        var timestampUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);
        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workoutId,
            UserId = "default-user",
            Status = status,
            Label = "Session",
            StartedAtUtc = timestampUtc,
            CreatedAtUtc = timestampUtc,
            UpdatedAtUtc = timestampUtc,
        });
    }

    private static void SeedLift(
        WeightLiftingDbContext dbContext,
        Guid liftId,
        string name,
        bool isActive)
    {
        dbContext.Lifts.Add(new LiftEntity
        {
            Id = liftId,
            Name = name,
            NameNormalized = Lift.NormalizeForUniqueLookup(name),
            IsActive = isActive,
            CreatedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc),
        });
    }
}
