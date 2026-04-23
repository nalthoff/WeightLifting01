using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.DeleteWorkoutSet;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Entities;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts.DeleteWorkoutSet;

public sealed class DeleteWorkoutSetCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncDeletesTargetedSet()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var workoutLiftEntryId = Guid.NewGuid();
        var setId = Guid.NewGuid();
        await SeedWorkoutSetAsync(dbContext, workoutId, workoutLiftEntryId, setId, Guid.NewGuid(), WorkoutStatus.InProgress, 1, 5, 225m);

        var handler = new DeleteWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new DeleteWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            SetId = setId,
        }, CancellationToken.None);

        Assert.Equal(DeleteWorkoutSetOutcome.Deleted, result.Outcome);
        Assert.False(await dbContext.WorkoutSets.AnyAsync(set => set.Id == setId));
    }

    [Fact]
    public async Task HandleAsyncReturnsNotFoundForMissingOrMismatchedResources()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var workoutLiftEntryId = Guid.NewGuid();
        var setId = Guid.NewGuid();
        await SeedWorkoutSetAsync(dbContext, workoutId, workoutLiftEntryId, setId, Guid.NewGuid(), WorkoutStatus.InProgress, 1, 5, 225m);

        var handler = new DeleteWorkoutSetCommandHandler(dbContext);

        var missingWorkout = await handler.HandleAsync(new DeleteWorkoutSetCommand
        {
            WorkoutId = Guid.NewGuid(),
            WorkoutLiftEntryId = workoutLiftEntryId,
            SetId = setId,
        }, CancellationToken.None);

        var missingLiftEntry = await handler.HandleAsync(new DeleteWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = Guid.NewGuid(),
            SetId = setId,
        }, CancellationToken.None);

        var missingSet = await handler.HandleAsync(new DeleteWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            SetId = Guid.NewGuid(),
        }, CancellationToken.None);

        Assert.Equal(DeleteWorkoutSetOutcome.NotFound, missingWorkout.Outcome);
        Assert.Equal(DeleteWorkoutSetOutcome.NotFound, missingLiftEntry.Outcome);
        Assert.Equal(DeleteWorkoutSetOutcome.NotFound, missingSet.Outcome);
    }

    [Fact]
    public async Task HandleAsyncReturnsConflictWhenWorkoutIsNotInProgress()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var workoutLiftEntryId = Guid.NewGuid();
        var setId = Guid.NewGuid();
        await SeedWorkoutSetAsync(dbContext, workoutId, workoutLiftEntryId, setId, Guid.NewGuid(), WorkoutStatus.Completed, 1, 5, 225m);

        var handler = new DeleteWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new DeleteWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            SetId = setId,
        }, CancellationToken.None);

        Assert.Equal(DeleteWorkoutSetOutcome.Conflict, result.Outcome);
        Assert.True(await dbContext.WorkoutSets.AnyAsync(set => set.Id == setId));
    }

    [Fact]
    public async Task HandleAsyncOnlyDeletesSetWithinTargetLiftEntryForDuplicateLiftEntries()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var sharedLiftId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        var firstSetId = Guid.NewGuid();
        var secondSetId = Guid.NewGuid();

        await SeedWorkoutSetAsync(dbContext, workoutId, firstEntryId, firstSetId, sharedLiftId, WorkoutStatus.InProgress, 1, 8, 155m, 1);
        await SeedWorkoutSetAsync(dbContext, workoutId, secondEntryId, secondSetId, sharedLiftId, WorkoutStatus.InProgress, 1, 10, 135m, 2);

        var handler = new DeleteWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new DeleteWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = firstEntryId,
            SetId = firstSetId,
        }, CancellationToken.None);

        Assert.Equal(DeleteWorkoutSetOutcome.Deleted, result.Outcome);
        Assert.False(await dbContext.WorkoutSets.AnyAsync(set => set.Id == firstSetId));
        Assert.True(await dbContext.WorkoutSets.AnyAsync(set => set.Id == secondSetId));
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }

    private static async Task SeedWorkoutSetAsync(
        WeightLiftingDbContext dbContext,
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
        if (!await dbContext.Workouts.AnyAsync(workout => workout.Id == workoutId))
        {
            SeedWorkout(dbContext, workoutId, status);
        }

        if (!await dbContext.Lifts.AnyAsync(lift => lift.Id == liftId))
        {
            SeedLift(dbContext, liftId, $"Lift-{position}");
        }

        if (!await dbContext.WorkoutLiftEntries.AnyAsync(entry => entry.Id == workoutLiftEntryId))
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
            CompletedAtUtc = status == WorkoutStatus.Completed ? timestampUtc.AddMinutes(30) : null,
            CreatedAtUtc = timestampUtc,
            UpdatedAtUtc = timestampUtc,
        });
    }

    private static void SeedLift(
        WeightLiftingDbContext dbContext,
        Guid liftId,
        string name)
    {
        dbContext.Lifts.Add(new LiftEntity
        {
            Id = liftId,
            Name = name,
            NameNormalized = Lift.NormalizeForUniqueLookup(name),
            IsActive = true,
            CreatedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc),
        });
    }
}
