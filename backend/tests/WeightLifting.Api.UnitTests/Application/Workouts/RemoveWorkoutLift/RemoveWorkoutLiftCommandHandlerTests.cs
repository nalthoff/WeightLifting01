using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.RemoveWorkoutLift;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts.RemoveWorkoutLift;

public sealed class RemoveWorkoutLiftCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncRemovesExistingWorkoutLiftEntry()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var removableEntryId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.InProgress);
        SeedEntry(dbContext, removableEntryId, workoutId, Guid.NewGuid(), "Front Squat", 1);
        await dbContext.SaveChangesAsync();

        var handler = new RemoveWorkoutLiftCommandHandler(dbContext);
        var result = await handler.HandleAsync(new RemoveWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = removableEntryId,
        }, CancellationToken.None);

        Assert.Equal(RemoveWorkoutLiftOutcome.Removed, result.Outcome);
        Assert.Equal(removableEntryId, result.WorkoutLiftEntryId);
        Assert.False(await dbContext.WorkoutLiftEntries.AnyAsync(entry => entry.Id == removableEntryId));
    }

    [Fact]
    public async Task HandleAsyncWhenWorkoutIsMissingReturnsNotFound()
    {
        await using var dbContext = CreateDbContext();
        var handler = new RemoveWorkoutLiftCommandHandler(dbContext);

        var result = await handler.HandleAsync(new RemoveWorkoutLiftCommand
        {
            WorkoutId = Guid.NewGuid(),
            WorkoutLiftEntryId = Guid.NewGuid(),
        }, CancellationToken.None);

        Assert.Equal(RemoveWorkoutLiftOutcome.NotFound, result.Outcome);
        Assert.Equal(Guid.Empty, result.WorkoutLiftEntryId);
    }

    [Fact]
    public async Task HandleAsyncWhenWorkoutLiftEntryIsMissingReturnsNotFound()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.InProgress);
        await dbContext.SaveChangesAsync();

        var handler = new RemoveWorkoutLiftCommandHandler(dbContext);
        var result = await handler.HandleAsync(new RemoveWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = Guid.NewGuid(),
        }, CancellationToken.None);

        Assert.Equal(RemoveWorkoutLiftOutcome.NotFound, result.Outcome);
        Assert.Equal(Guid.Empty, result.WorkoutLiftEntryId);
    }

    [Fact]
    public async Task HandleAsyncWhenWorkoutNotInProgressReturnsConflict()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var removableEntryId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.Completed);
        SeedEntry(dbContext, removableEntryId, workoutId, Guid.NewGuid(), "Deadlift", 1);
        await dbContext.SaveChangesAsync();

        var handler = new RemoveWorkoutLiftCommandHandler(dbContext);
        var result = await handler.HandleAsync(new RemoveWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = removableEntryId,
        }, CancellationToken.None);

        Assert.Equal(RemoveWorkoutLiftOutcome.Conflict, result.Outcome);
        Assert.Equal(Guid.Empty, result.WorkoutLiftEntryId);
        Assert.True(await dbContext.WorkoutLiftEntries.AnyAsync(entry => entry.Id == removableEntryId));
    }

    [Fact]
    public async Task HandleAsyncWithDuplicateLiftEntriesRemovesOnlySelectedEntry()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var sharedLiftId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.InProgress);
        SeedEntry(dbContext, firstEntryId, workoutId, sharedLiftId, "Bench Press", 1);
        SeedEntry(dbContext, secondEntryId, workoutId, sharedLiftId, "Bench Press", 2);
        await dbContext.SaveChangesAsync();

        var handler = new RemoveWorkoutLiftCommandHandler(dbContext);
        var result = await handler.HandleAsync(new RemoveWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = secondEntryId,
        }, CancellationToken.None);

        var remainingEntries = await dbContext.WorkoutLiftEntries
            .Where(entry => entry.WorkoutId == workoutId)
            .ToListAsync();

        Assert.Equal(RemoveWorkoutLiftOutcome.Removed, result.Outcome);
        Assert.Equal(secondEntryId, result.WorkoutLiftEntryId);
        Assert.Single(remainingEntries);
        Assert.Equal(firstEntryId, remainingEntries[0].Id);
        Assert.Equal(sharedLiftId, remainingEntries[0].LiftId);
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
            CompletedAtUtc = status == WorkoutStatus.Completed ? timestampUtc.AddMinutes(30) : null,
        });
    }

    private static void SeedEntry(
        WeightLiftingDbContext dbContext,
        Guid entryId,
        Guid workoutId,
        Guid liftId,
        string displayName,
        int position)
    {
        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = entryId,
            WorkoutId = workoutId,
            LiftId = liftId,
            DisplayName = displayName,
            AddedAtUtc = new DateTime(2026, 4, 22, 12, 5, 0, DateTimeKind.Utc).AddMinutes(position),
            Position = position,
        });
    }
}
