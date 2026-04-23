using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.ReorderWorkoutLifts;
using WeightLifting.Api.Application.Workouts.Queries.ListWorkoutLifts;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class ReorderWorkoutLiftsIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task ReorderWorkoutLiftsPersistsUpdatedPositionsWithContiguousResequencing()
    {
        var workoutId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        var thirdEntryId = Guid.NewGuid();
        await SeedWorkoutAsync(workoutId, WorkoutStatus.InProgress);
        await SeedEntryAsync(firstEntryId, workoutId, Guid.NewGuid(), "Front Squat", 1);
        await SeedEntryAsync(secondEntryId, workoutId, Guid.NewGuid(), "Bench Press", 2);
        await SeedEntryAsync(thirdEntryId, workoutId, Guid.NewGuid(), "Deadlift", 3);

        var handler = new ReorderWorkoutLiftsCommandHandler(dbContext);
        var listHelper = new ListWorkoutLiftsQueryHelper(dbContext);

        var result = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = workoutId,
            OrderedWorkoutLiftEntryIds = [thirdEntryId, firstEntryId, secondEntryId],
        }, CancellationToken.None);

        var listed = await listHelper.GetAsync(workoutId, CancellationToken.None);

        Assert.Equal(ReorderWorkoutLiftsOutcome.Reordered, result.Outcome);
        Assert.Equal([thirdEntryId, firstEntryId, secondEntryId], listed.Select(item => item.Id).ToArray());
        Assert.Equal([1, 2, 3], listed.Select(item => item.Position).ToArray());
    }

    [Fact]
    public async Task ReorderWorkoutLiftsWithDuplicateLiftEntriesPreservesIdentity()
    {
        var workoutId = Guid.NewGuid();
        var sharedLiftId = Guid.NewGuid();
        var firstDuplicateEntryId = Guid.NewGuid();
        var uniqueEntryId = Guid.NewGuid();
        var secondDuplicateEntryId = Guid.NewGuid();
        await SeedWorkoutAsync(workoutId, WorkoutStatus.InProgress);
        await SeedEntryAsync(firstDuplicateEntryId, workoutId, sharedLiftId, "Bench Press", 1);
        await SeedEntryAsync(uniqueEntryId, workoutId, Guid.NewGuid(), "Deadlift", 2);
        await SeedEntryAsync(secondDuplicateEntryId, workoutId, sharedLiftId, "Bench Press", 3);

        var handler = new ReorderWorkoutLiftsCommandHandler(dbContext);
        var listHelper = new ListWorkoutLiftsQueryHelper(dbContext);

        var result = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = workoutId,
            OrderedWorkoutLiftEntryIds = [secondDuplicateEntryId, firstDuplicateEntryId, uniqueEntryId],
        }, CancellationToken.None);

        var listed = await listHelper.GetAsync(workoutId, CancellationToken.None);

        Assert.Equal(ReorderWorkoutLiftsOutcome.Reordered, result.Outcome);
        Assert.Equal([secondDuplicateEntryId, firstDuplicateEntryId, uniqueEntryId], listed.Select(item => item.Id).ToArray());
        Assert.Equal(2, listed.Count(item => item.LiftId == sharedLiftId));
    }

    [Fact]
    public async Task ReorderWorkoutLiftsReturnsExpectedFailureOutcomes()
    {
        var inProgressWorkoutId = Guid.NewGuid();
        var completedWorkoutId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        await SeedWorkoutAsync(inProgressWorkoutId, WorkoutStatus.InProgress);
        await SeedWorkoutAsync(completedWorkoutId, WorkoutStatus.Completed);
        await SeedEntryAsync(firstEntryId, inProgressWorkoutId, Guid.NewGuid(), "Row", 1);
        await SeedEntryAsync(secondEntryId, inProgressWorkoutId, Guid.NewGuid(), "Pull Up", 2);

        var handler = new ReorderWorkoutLiftsCommandHandler(dbContext);

        var missingWorkout = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = Guid.NewGuid(),
            OrderedWorkoutLiftEntryIds = [firstEntryId, secondEntryId],
        }, CancellationToken.None);

        var conflict = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = completedWorkoutId,
            OrderedWorkoutLiftEntryIds = [Guid.NewGuid()],
        }, CancellationToken.None);

        var validationFailed = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = inProgressWorkoutId,
            OrderedWorkoutLiftEntryIds = [firstEntryId, firstEntryId],
        }, CancellationToken.None);

        Assert.Equal(ReorderWorkoutLiftsOutcome.NotFound, missingWorkout.Outcome);
        Assert.Equal(ReorderWorkoutLiftsOutcome.Conflict, conflict.Outcome);
        Assert.Equal(ReorderWorkoutLiftsOutcome.ValidationFailed, validationFailed.Outcome);
    }

    [Fact]
    public async Task ReorderWorkoutLiftsDoesNotMutateHistoricalCompletedWorkouts()
    {
        var inProgressWorkoutId = Guid.NewGuid();
        var completedWorkoutId = Guid.NewGuid();
        var inProgressFirstEntryId = Guid.NewGuid();
        var inProgressSecondEntryId = Guid.NewGuid();
        var historicalFirstEntryId = Guid.NewGuid();
        var historicalSecondEntryId = Guid.NewGuid();
        await SeedWorkoutAsync(inProgressWorkoutId, WorkoutStatus.InProgress);
        await SeedWorkoutAsync(completedWorkoutId, WorkoutStatus.Completed);
        await SeedEntryAsync(inProgressFirstEntryId, inProgressWorkoutId, Guid.NewGuid(), "Front Squat", 1);
        await SeedEntryAsync(inProgressSecondEntryId, inProgressWorkoutId, Guid.NewGuid(), "Bench Press", 2);
        await SeedEntryAsync(historicalFirstEntryId, completedWorkoutId, Guid.NewGuid(), "Row", 1);
        await SeedEntryAsync(historicalSecondEntryId, completedWorkoutId, Guid.NewGuid(), "Deadlift", 2);

        var handler = new ReorderWorkoutLiftsCommandHandler(dbContext);

        var result = await handler.HandleAsync(new ReorderWorkoutLiftsCommand
        {
            WorkoutId = inProgressWorkoutId,
            OrderedWorkoutLiftEntryIds = [inProgressSecondEntryId, inProgressFirstEntryId],
        }, CancellationToken.None);

        var historicalOrder = await dbContext.WorkoutLiftEntries
            .Where(entry => entry.WorkoutId == completedWorkoutId)
            .OrderBy(entry => entry.Position)
            .Select(entry => entry.Id)
            .ToListAsync();

        Assert.Equal(ReorderWorkoutLiftsOutcome.Reordered, result.Outcome);
        Assert.Equal([historicalFirstEntryId, historicalSecondEntryId], historicalOrder);
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

    private async Task SeedWorkoutAsync(Guid workoutId, WorkoutStatus status)
    {
        var timestampUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);
        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workoutId,
            UserId = "default-user",
            Status = status,
            Label = "Session",
            StartedAtUtc = timestampUtc,
            CompletedAtUtc = status == WorkoutStatus.Completed ? timestampUtc.AddMinutes(30) : null,
            CreatedAtUtc = timestampUtc,
            UpdatedAtUtc = timestampUtc,
        });

        await dbContext.SaveChangesAsync();
    }

    private async Task SeedEntryAsync(
        Guid entryId,
        Guid workoutId,
        Guid liftId,
        string displayName,
        int position)
    {
        var existingLift = await dbContext.Lifts.SingleOrDefaultAsync(entity => entity.Id == liftId);
        if (existingLift is null)
        {
            dbContext.Lifts.Add(new LiftEntity
            {
                Id = liftId,
                Name = displayName,
                NameNormalized = Lift.NormalizeForUniqueLookup(displayName),
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc),
            });
        }

        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = entryId,
            WorkoutId = workoutId,
            LiftId = liftId,
            DisplayName = displayName,
            AddedAtUtc = new DateTime(2026, 4, 22, 12, 5, 0, DateTimeKind.Utc).AddMinutes(position),
            Position = position,
        });

        await dbContext.SaveChangesAsync();
    }
}
