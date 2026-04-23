using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutSet;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Entities;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts.UpdateWorkoutSet;

public sealed class UpdateWorkoutSetCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncUpdatesRepsAndWeightWithoutChangingSetNumber()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var workoutLiftEntryId = Guid.NewGuid();
        var setId = Guid.NewGuid();
        await SeedWorkoutSetAsync(dbContext, workoutId, workoutLiftEntryId, setId, Guid.NewGuid(), WorkoutStatus.InProgress, 1, 5, 225m);

        var handler = new UpdateWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new UpdateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            SetId = setId,
            Reps = 7,
            Weight = 230m,
        }, CancellationToken.None);

        var persisted = await dbContext.WorkoutSets.SingleAsync(set => set.Id == setId);
        Assert.Equal(UpdateWorkoutSetOutcome.Updated, result.Outcome);
        Assert.Equal(1, result.Set!.SetNumber);
        Assert.Equal(7, persisted.Reps);
        Assert.Equal(230m, persisted.Weight);
        Assert.Equal(1, persisted.SetNumber);
    }

    [Fact]
    public async Task HandleAsyncReturnsValidationFailedForInvalidPayload()
    {
        await using var dbContext = CreateDbContext();
        var handler = new UpdateWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new UpdateWorkoutSetCommand
        {
            WorkoutId = Guid.Empty,
            WorkoutLiftEntryId = Guid.Empty,
            SetId = Guid.Empty,
            Reps = 0,
            Weight = -1m,
        }, CancellationToken.None);

        Assert.Equal(UpdateWorkoutSetOutcome.ValidationFailed, result.Outcome);
        Assert.Contains("Workout id is required.", result.Errors["workoutId"]);
        Assert.Contains("Workout lift entry id is required.", result.Errors["workoutLiftEntryId"]);
        Assert.Contains("Set id is required.", result.Errors["setId"]);
        Assert.Contains("Reps must be greater than zero.", result.Errors["reps"]);
        Assert.Contains("Weight must be greater than or equal to zero when provided.", result.Errors["weight"]);
    }

    [Fact]
    public async Task HandleAsyncReturnsNotFoundForMissingOrMismatchedResources()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var workoutLiftEntryId = Guid.NewGuid();
        var setId = Guid.NewGuid();
        await SeedWorkoutSetAsync(dbContext, workoutId, workoutLiftEntryId, setId, Guid.NewGuid(), WorkoutStatus.InProgress, 1, 5, 225m);

        var handler = new UpdateWorkoutSetCommandHandler(dbContext);

        var missingWorkout = await handler.HandleAsync(new UpdateWorkoutSetCommand
        {
            WorkoutId = Guid.NewGuid(),
            WorkoutLiftEntryId = workoutLiftEntryId,
            SetId = setId,
            Reps = 6,
        }, CancellationToken.None);

        var missingLiftEntry = await handler.HandleAsync(new UpdateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = Guid.NewGuid(),
            SetId = setId,
            Reps = 6,
        }, CancellationToken.None);

        var missingSet = await handler.HandleAsync(new UpdateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            SetId = Guid.NewGuid(),
            Reps = 6,
        }, CancellationToken.None);

        Assert.Equal(UpdateWorkoutSetOutcome.NotFound, missingWorkout.Outcome);
        Assert.Equal(UpdateWorkoutSetOutcome.NotFound, missingLiftEntry.Outcome);
        Assert.Equal(UpdateWorkoutSetOutcome.NotFound, missingSet.Outcome);
    }

    [Fact]
    public async Task HandleAsyncReturnsConflictWhenWorkoutIsNotInProgress()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var workoutLiftEntryId = Guid.NewGuid();
        var setId = Guid.NewGuid();
        await SeedWorkoutSetAsync(dbContext, workoutId, workoutLiftEntryId, setId, Guid.NewGuid(), WorkoutStatus.Completed, 1, 5, 225m);

        var handler = new UpdateWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new UpdateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            SetId = setId,
            Reps = 10,
            Weight = 205m,
        }, CancellationToken.None);

        Assert.Equal(UpdateWorkoutSetOutcome.Conflict, result.Outcome);
        Assert.Contains("Workout must be in progress to update sets.", result.Errors["workout"]);
    }

    [Fact]
    public async Task HandleAsyncOnlyUpdatesSetWithinTargetLiftEntryForDuplicateLiftEntries()
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

        var handler = new UpdateWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new UpdateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = firstEntryId,
            SetId = firstSetId,
            Reps = 6,
            Weight = 165m,
        }, CancellationToken.None);

        var firstSet = await dbContext.WorkoutSets.SingleAsync(set => set.Id == firstSetId);
        var secondSet = await dbContext.WorkoutSets.SingleAsync(set => set.Id == secondSetId);

        Assert.Equal(UpdateWorkoutSetOutcome.Updated, result.Outcome);
        Assert.Equal(6, firstSet.Reps);
        Assert.Equal(165m, firstSet.Weight);
        Assert.Equal(10, secondSet.Reps);
        Assert.Equal(135m, secondSet.Weight);
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
