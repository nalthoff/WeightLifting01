using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.AddWorkoutSet;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Entities;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts.AddWorkoutSet;

public sealed class AddWorkoutSetCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncCreatesSetForInProgressWorkoutWithNextSetNumber()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var workoutLiftEntryId = Guid.NewGuid();
        await SeedWorkoutLiftEntryAsync(dbContext, workoutId, workoutLiftEntryId, Guid.NewGuid(), WorkoutStatus.InProgress, "Front Squat");

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

        Assert.Equal(AddWorkoutSetOutcome.Created, first.Outcome);
        Assert.Equal(AddWorkoutSetOutcome.Created, second.Outcome);
        Assert.Equal(1, first.Set!.SetNumber);
        Assert.Equal(2, second.Set!.SetNumber);
        Assert.Null(second.Set.Weight);
    }

    [Fact]
    public async Task HandleAsyncAssignsIndependentSetNumbersPerDuplicateLiftEntry()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var sharedLiftId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        await SeedWorkoutLiftEntryAsync(dbContext, workoutId, firstEntryId, sharedLiftId, WorkoutStatus.InProgress, "Bench Press", position: 1);
        await SeedWorkoutLiftEntryAsync(dbContext, workoutId, secondEntryId, sharedLiftId, WorkoutStatus.InProgress, "Bench Press", position: 2);

        var handler = new AddWorkoutSetCommandHandler(dbContext);

        var firstEntrySet = await handler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = firstEntryId,
            Reps = 8,
            Weight = 155m,
        }, CancellationToken.None);

        var secondEntrySet = await handler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = secondEntryId,
            Reps = 10,
            Weight = 135m,
        }, CancellationToken.None);

        Assert.Equal(1, firstEntrySet.Set!.SetNumber);
        Assert.Equal(1, secondEntrySet.Set!.SetNumber);
    }

    [Fact]
    public async Task HandleAsyncReturnsNotFoundWhenWorkoutOrLiftEntryIsMissingOrMismatched()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var otherWorkoutId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        await SeedWorkoutLiftEntryAsync(dbContext, workoutId, entryId, Guid.NewGuid(), WorkoutStatus.InProgress, "Deadlift");
        SeedWorkout(dbContext, otherWorkoutId, WorkoutStatus.InProgress);
        await dbContext.SaveChangesAsync();

        var handler = new AddWorkoutSetCommandHandler(dbContext);

        var missingWorkout = await handler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = Guid.NewGuid(),
            WorkoutLiftEntryId = entryId,
            Reps = 5,
        }, CancellationToken.None);

        var missingEntry = await handler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = Guid.NewGuid(),
            Reps = 5,
        }, CancellationToken.None);

        var mismatchedOwnership = await handler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = otherWorkoutId,
            WorkoutLiftEntryId = entryId,
            Reps = 5,
        }, CancellationToken.None);

        Assert.Equal(AddWorkoutSetOutcome.NotFound, missingWorkout.Outcome);
        Assert.Equal(AddWorkoutSetOutcome.NotFound, missingEntry.Outcome);
        Assert.Equal(AddWorkoutSetOutcome.NotFound, mismatchedOwnership.Outcome);
    }

    [Fact]
    public async Task HandleAsyncReturnsConflictWhenWorkoutIsNotInProgress()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        await SeedWorkoutLiftEntryAsync(dbContext, workoutId, entryId, Guid.NewGuid(), WorkoutStatus.Completed, "Row");

        var handler = new AddWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = entryId,
            Reps = 10,
            Weight = 95m,
        }, CancellationToken.None);

        Assert.Equal(AddWorkoutSetOutcome.Conflict, result.Outcome);
        Assert.Contains("Workout must be in progress to add sets.", result.Errors["workout"]);
    }

    [Fact]
    public async Task HandleAsyncReturnsValidationFailedForInvalidPayload()
    {
        await using var dbContext = CreateDbContext();
        var handler = new AddWorkoutSetCommandHandler(dbContext);

        var result = await handler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = Guid.Empty,
            WorkoutLiftEntryId = Guid.Empty,
            Reps = 0,
            Weight = -1m,
        }, CancellationToken.None);

        Assert.Equal(AddWorkoutSetOutcome.ValidationFailed, result.Outcome);
        Assert.Contains("Workout id is required.", result.Errors["workoutId"]);
        Assert.Contains("Workout lift entry id is required.", result.Errors["workoutLiftEntryId"]);
        Assert.Contains("Reps must be greater than zero.", result.Errors["reps"]);
        Assert.Contains("Weight must be greater than or equal to zero when provided.", result.Errors["weight"]);
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }

    private static async Task SeedWorkoutLiftEntryAsync(
        WeightLiftingDbContext dbContext,
        Guid workoutId,
        Guid workoutLiftEntryId,
        Guid liftId,
        WorkoutStatus status,
        string liftName,
        int position = 1)
    {
        if (!await dbContext.Workouts.AnyAsync(workout => workout.Id == workoutId))
        {
            SeedWorkout(dbContext, workoutId, status);
        }

        if (!await dbContext.Lifts.AnyAsync(lift => lift.Id == liftId))
        {
            SeedLift(dbContext, liftId, liftName);
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
