using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.DeleteWorkoutSet;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Entities;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class DeleteWorkoutSetIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task DeleteWorkoutSetRemovesTargetedSetOnly()
    {
        var workoutId = Guid.NewGuid();
        var workoutLiftEntryId = Guid.NewGuid();
        var setOneId = Guid.NewGuid();
        var setTwoId = Guid.NewGuid();
        var liftId = Guid.NewGuid();
        await SeedWorkoutSetAsync(workoutId, workoutLiftEntryId, setOneId, liftId, WorkoutStatus.InProgress, 1, 5, 225m);
        await SeedWorkoutSetAsync(workoutId, workoutLiftEntryId, setTwoId, liftId, WorkoutStatus.InProgress, 2, 4, 230m);

        var handler = new DeleteWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new DeleteWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            SetId = setOneId,
        }, CancellationToken.None);

        Assert.Equal(DeleteWorkoutSetOutcome.Deleted, result.Outcome);
        Assert.False(await dbContext.WorkoutSets.AnyAsync(set => set.Id == setOneId));
        Assert.True(await dbContext.WorkoutSets.AnyAsync(set => set.Id == setTwoId));
    }

    [Fact]
    public async Task DeleteWorkoutSetForDuplicateLiftEntriesIsIsolatedToTargetEntry()
    {
        var workoutId = Guid.NewGuid();
        var sharedLiftId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        var firstSetId = Guid.NewGuid();
        var secondSetId = Guid.NewGuid();

        await SeedWorkoutSetAsync(workoutId, firstEntryId, firstSetId, sharedLiftId, WorkoutStatus.InProgress, 1, 8, 155m, 1);
        await SeedWorkoutSetAsync(workoutId, secondEntryId, secondSetId, sharedLiftId, WorkoutStatus.InProgress, 1, 10, 135m, 2);

        var handler = new DeleteWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new DeleteWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = secondEntryId,
            SetId = secondSetId,
        }, CancellationToken.None);

        Assert.Equal(DeleteWorkoutSetOutcome.Deleted, result.Outcome);
        Assert.True(await dbContext.WorkoutSets.AnyAsync(set => set.Id == firstSetId));
        Assert.False(await dbContext.WorkoutSets.AnyAsync(set => set.Id == secondSetId));
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
