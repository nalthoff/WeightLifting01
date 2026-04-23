using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.AddWorkoutSet;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class AddWorkoutSetIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task AddWorkoutSetPersistsRowsAndSetNumbersForTargetedEntry()
    {
        var workoutId = Guid.NewGuid();
        var workoutLiftEntryId = Guid.NewGuid();
        await SeedWorkoutLiftEntryAsync(workoutId, workoutLiftEntryId, Guid.NewGuid(), WorkoutStatus.InProgress, "Front Squat");

        var handler = new AddWorkoutSetCommandHandler(dbContext);

        var first = await handler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            Reps = 5,
            Weight = 225m,
        }, CancellationToken.None);

        var second = await handler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            Reps = 6,
            Weight = null,
        }, CancellationToken.None);

        var persisted = await dbContext.WorkoutSets
            .Where(set => set.WorkoutLiftEntryId == workoutLiftEntryId)
            .OrderBy(set => set.SetNumber)
            .ToListAsync();

        Assert.Equal(AddWorkoutSetOutcome.Created, first.Outcome);
        Assert.Equal(AddWorkoutSetOutcome.Created, second.Outcome);
        Assert.Equal([1, 2], persisted.Select(set => set.SetNumber).ToArray());
        Assert.Equal([5, 6], persisted.Select(set => set.Reps).ToArray());
        Assert.Null(persisted[1].Weight);
    }

    [Fact]
    public async Task AddWorkoutSetForDuplicateLiftEntriesKeepsSetListsIsolated()
    {
        var workoutId = Guid.NewGuid();
        var sharedLiftId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        await SeedWorkoutLiftEntryAsync(workoutId, firstEntryId, sharedLiftId, WorkoutStatus.InProgress, "Bench Press", 1);
        await SeedWorkoutLiftEntryAsync(workoutId, secondEntryId, sharedLiftId, WorkoutStatus.InProgress, "Bench Press", 2);

        var handler = new AddWorkoutSetCommandHandler(dbContext);

        await handler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = firstEntryId,
            Reps = 8,
            Weight = 155m,
        }, CancellationToken.None);

        await handler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = firstEntryId,
            Reps = 7,
            Weight = 165m,
        }, CancellationToken.None);

        await handler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = secondEntryId,
            Reps = 10,
            Weight = 135m,
        }, CancellationToken.None);

        var firstEntrySetNumbers = await dbContext.WorkoutSets
            .Where(set => set.WorkoutLiftEntryId == firstEntryId)
            .OrderBy(set => set.SetNumber)
            .Select(set => set.SetNumber)
            .ToListAsync();

        var secondEntrySetNumbers = await dbContext.WorkoutSets
            .Where(set => set.WorkoutLiftEntryId == secondEntryId)
            .OrderBy(set => set.SetNumber)
            .Select(set => set.SetNumber)
            .ToListAsync();

        Assert.Equal([1, 2], firstEntrySetNumbers);
        Assert.Equal([1], secondEntrySetNumbers);
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

    private async Task SeedWorkoutLiftEntryAsync(
        Guid workoutId,
        Guid workoutLiftEntryId,
        Guid liftId,
        WorkoutStatus status,
        string liftName,
        int position = 1)
    {
        var existingWorkout = await dbContext.Workouts.SingleOrDefaultAsync(workout => workout.Id == workoutId);
        if (existingWorkout is null)
        {
            dbContext.Workouts.Add(new WorkoutEntity
            {
                Id = workoutId,
                UserId = "default-user",
                Status = status,
                Label = "Session",
                StartedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc),
                CompletedAtUtc = status == WorkoutStatus.Completed ? new DateTime(2026, 4, 22, 12, 30, 0, DateTimeKind.Utc) : null,
                CreatedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc),
                UpdatedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc),
            });
        }

        var existingLift = await dbContext.Lifts.SingleOrDefaultAsync(lift => lift.Id == liftId);
        if (existingLift is null)
        {
            dbContext.Lifts.Add(new LiftEntity
            {
                Id = liftId,
                Name = liftName,
                NameNormalized = Lift.NormalizeForUniqueLookup(liftName),
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc),
            });
        }

        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = workoutLiftEntryId,
            WorkoutId = workoutId,
            LiftId = liftId,
            DisplayName = liftName,
            AddedAtUtc = new DateTime(2026, 4, 22, 12, 5, 0, DateTimeKind.Utc).AddMinutes(position),
            Position = position,
        });

        await dbContext.SaveChangesAsync();
    }
}
