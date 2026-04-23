using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.ReorderWorkoutLifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts.ReorderWorkoutLifts;

public sealed class ReorderWorkoutLiftsCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncReordersEntriesAndResequencesPositions()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        var thirdEntryId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.InProgress);
        SeedEntry(dbContext, firstEntryId, workoutId, Guid.NewGuid(), "Front Squat", 1);
        SeedEntry(dbContext, secondEntryId, workoutId, Guid.NewGuid(), "Bench Press", 2);
        SeedEntry(dbContext, thirdEntryId, workoutId, Guid.NewGuid(), "Deadlift", 3);
        await dbContext.SaveChangesAsync();

        var handler = new ReorderWorkoutLiftsCommandHandler(dbContext);
        var result = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = workoutId,
            OrderedWorkoutLiftEntryIds = [thirdEntryId, firstEntryId, secondEntryId],
        }, CancellationToken.None);

        var persistedEntries = await dbContext.WorkoutLiftEntries
            .Where(entry => entry.WorkoutId == workoutId)
            .OrderBy(entry => entry.Position)
            .ToListAsync();

        Assert.Equal(ReorderWorkoutLiftsOutcome.Reordered, result.Outcome);
        Assert.Equal([thirdEntryId, firstEntryId, secondEntryId], result.Items.Select(item => item.Id).ToArray());
        Assert.Equal([1, 2, 3], persistedEntries.Select(entry => entry.Position).ToArray());
        Assert.Equal([thirdEntryId, firstEntryId, secondEntryId], persistedEntries.Select(entry => entry.Id).ToArray());
    }

    [Fact]
    public async Task HandleAsyncWhenWorkoutNotInProgressReturnsConflict()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.Completed);
        SeedEntry(dbContext, firstEntryId, workoutId, Guid.NewGuid(), "Front Squat", 1);
        SeedEntry(dbContext, secondEntryId, workoutId, Guid.NewGuid(), "Bench Press", 2);
        await dbContext.SaveChangesAsync();

        var handler = new ReorderWorkoutLiftsCommandHandler(dbContext);
        var result = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = workoutId,
            OrderedWorkoutLiftEntryIds = [secondEntryId, firstEntryId],
        }, CancellationToken.None);

        var persistedOrder = await dbContext.WorkoutLiftEntries
            .Where(entry => entry.WorkoutId == workoutId)
            .OrderBy(entry => entry.Position)
            .Select(entry => entry.Id)
            .ToListAsync();

        Assert.Equal(ReorderWorkoutLiftsOutcome.Conflict, result.Outcome);
        Assert.Equal([firstEntryId, secondEntryId], persistedOrder);
    }

    [Fact]
    public async Task HandleAsyncWithDuplicateLiftEntriesReordersOnlyRequestedInstanceOrder()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var sharedLiftId = Guid.NewGuid();
        var firstDuplicateEntryId = Guid.NewGuid();
        var uniqueEntryId = Guid.NewGuid();
        var secondDuplicateEntryId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.InProgress);
        SeedEntry(dbContext, firstDuplicateEntryId, workoutId, sharedLiftId, "Bench Press", 1);
        SeedEntry(dbContext, uniqueEntryId, workoutId, Guid.NewGuid(), "Deadlift", 2);
        SeedEntry(dbContext, secondDuplicateEntryId, workoutId, sharedLiftId, "Bench Press", 3);
        await dbContext.SaveChangesAsync();

        var handler = new ReorderWorkoutLiftsCommandHandler(dbContext);
        var result = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = workoutId,
            OrderedWorkoutLiftEntryIds = [secondDuplicateEntryId, firstDuplicateEntryId, uniqueEntryId],
        }, CancellationToken.None);

        var persistedEntries = await dbContext.WorkoutLiftEntries
            .Where(entry => entry.WorkoutId == workoutId)
            .OrderBy(entry => entry.Position)
            .ToListAsync();

        Assert.Equal(ReorderWorkoutLiftsOutcome.Reordered, result.Outcome);
        Assert.Equal([secondDuplicateEntryId, firstDuplicateEntryId, uniqueEntryId], persistedEntries.Select(entry => entry.Id).ToArray());
        Assert.Equal(2, persistedEntries.Count(entry => entry.LiftId == sharedLiftId));
    }

    [Fact]
    public async Task HandleAsyncReturnsNotFoundWhenWorkoutIsMissing()
    {
        await using var dbContext = CreateDbContext();
        var handler = new ReorderWorkoutLiftsCommandHandler(dbContext);

        var result = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = Guid.NewGuid(),
            OrderedWorkoutLiftEntryIds = [Guid.NewGuid()],
        }, CancellationToken.None);

        Assert.Equal(ReorderWorkoutLiftsOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task HandleAsyncReturnsNotFoundWhenRequestedEntryDoesNotBelongToWorkout()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.InProgress);
        SeedEntry(dbContext, firstEntryId, workoutId, Guid.NewGuid(), "Front Squat", 1);
        SeedEntry(dbContext, secondEntryId, workoutId, Guid.NewGuid(), "Bench Press", 2);
        await dbContext.SaveChangesAsync();

        var handler = new ReorderWorkoutLiftsCommandHandler(dbContext);
        var result = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = workoutId,
            OrderedWorkoutLiftEntryIds = [firstEntryId, Guid.NewGuid()],
        }, CancellationToken.None);

        Assert.Equal(ReorderWorkoutLiftsOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task HandleAsyncReturnsValidationFailedWhenOrderedIdsContainDuplicates()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.InProgress);
        SeedEntry(dbContext, firstEntryId, workoutId, Guid.NewGuid(), "Front Squat", 1);
        SeedEntry(dbContext, secondEntryId, workoutId, Guid.NewGuid(), "Bench Press", 2);
        await dbContext.SaveChangesAsync();

        var handler = new ReorderWorkoutLiftsCommandHandler(dbContext);
        var result = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = workoutId,
            OrderedWorkoutLiftEntryIds = [firstEntryId, firstEntryId],
        }, CancellationToken.None);

        Assert.Equal(ReorderWorkoutLiftsOutcome.ValidationFailed, result.Outcome);
    }

    [Fact]
    public async Task HandleAsyncReturnsValidationFailedWhenOrderedIdsAreIncomplete()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        SeedWorkout(dbContext, workoutId, WorkoutStatus.InProgress);
        SeedEntry(dbContext, firstEntryId, workoutId, Guid.NewGuid(), "Front Squat", 1);
        SeedEntry(dbContext, secondEntryId, workoutId, Guid.NewGuid(), "Bench Press", 2);
        await dbContext.SaveChangesAsync();

        var handler = new ReorderWorkoutLiftsCommandHandler(dbContext);
        var result = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = workoutId,
            OrderedWorkoutLiftEntryIds = [firstEntryId],
        }, CancellationToken.None);

        Assert.Equal(ReorderWorkoutLiftsOutcome.ValidationFailed, result.Outcome);
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
