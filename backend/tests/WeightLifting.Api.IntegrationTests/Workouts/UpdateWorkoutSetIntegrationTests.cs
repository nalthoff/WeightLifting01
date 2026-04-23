using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutSet;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Entities;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class UpdateWorkoutSetIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task UpdateWorkoutSetPersistsTargetedSetChanges()
    {
        var workoutId = Guid.NewGuid();
        var workoutLiftEntryId = Guid.NewGuid();
        var setId = Guid.NewGuid();
        await SeedWorkoutSetAsync(workoutId, workoutLiftEntryId, setId, Guid.NewGuid(), WorkoutStatus.InProgress, 1, 5, 225m);

        var handler = new UpdateWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new UpdateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            SetId = setId,
            Reps = 4,
            Weight = 215m,
        }, CancellationToken.None);

        var persisted = await dbContext.WorkoutSets.SingleAsync(set => set.Id == setId);
        Assert.Equal(UpdateWorkoutSetOutcome.Updated, result.Outcome);
        Assert.Equal(4, persisted.Reps);
        Assert.Equal(215m, persisted.Weight);
        Assert.Equal(1, persisted.SetNumber);
    }

    [Fact]
    public async Task UpdateWorkoutSetForDuplicateLiftEntriesIsIsolatedToTargetEntry()
    {
        var workoutId = Guid.NewGuid();
        var sharedLiftId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        var firstSetId = Guid.NewGuid();
        var secondSetId = Guid.NewGuid();

        await SeedWorkoutSetAsync(workoutId, firstEntryId, firstSetId, sharedLiftId, WorkoutStatus.InProgress, 1, 8, 155m, 1);
        await SeedWorkoutSetAsync(workoutId, secondEntryId, secondSetId, sharedLiftId, WorkoutStatus.InProgress, 1, 10, 135m, 2);

        var handler = new UpdateWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new UpdateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = secondEntryId,
            SetId = secondSetId,
            Reps = 12,
            Weight = 140m,
        }, CancellationToken.None);

        var firstSet = await dbContext.WorkoutSets.SingleAsync(set => set.Id == firstSetId);
        var secondSet = await dbContext.WorkoutSets.SingleAsync(set => set.Id == secondSetId);

        Assert.Equal(UpdateWorkoutSetOutcome.Updated, result.Outcome);
        Assert.Equal(8, firstSet.Reps);
        Assert.Equal(155m, firstSet.Weight);
        Assert.Equal(12, secondSet.Reps);
        Assert.Equal(140m, secondSet.Weight);
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

    private async Task SeedWorkoutSetAsync(
        Guid workoutId,
        Guid workoutLiftEntryId,
        Guid setId,
        Guid liftId,
        WorkoutStatus status,
        int setNumber,
        int reps,
        decimal? weight,
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
                Name = $"Lift-{position}",
                NameNormalized = Lift.NormalizeForUniqueLookup($"Lift-{position}"),
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc),
            });
        }

        var existingEntry = await dbContext.WorkoutLiftEntries.SingleOrDefaultAsync(entry => entry.Id == workoutLiftEntryId);
        if (existingEntry is null)
        {
            dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
            {
                Id = workoutLiftEntryId,
                WorkoutId = workoutId,
                LiftId = liftId,
                DisplayName = $"Lift-{position}",
                AddedAtUtc = new DateTime(2026, 4, 22, 12, 5, 0, DateTimeKind.Utc).AddMinutes(position),
                Position = position,
            });
        }

        var nowUtc = new DateTime(2026, 4, 22, 12, 10, 0, DateTimeKind.Utc).AddMinutes(position);
        dbContext.WorkoutSets.Add(new WorkoutSetEntity
        {
            Id = setId,
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            SetNumber = setNumber,
            Reps = reps,
            Weight = weight,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc,
        });

        await dbContext.SaveChangesAsync();
    }
}
