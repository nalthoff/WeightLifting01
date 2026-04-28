using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.AddWorkoutLift;
using WeightLifting.Api.Application.Workouts.Commands.AddWorkoutSet;
using WeightLifting.Api.Application.Workouts.Commands.CreateHistoricalWorkout;
using WeightLifting.Api.Application.Workouts.Commands.TimingValidation;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Entities;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts;

public sealed class HistoricalWorkoutLiftSetParityTests
{
    [Fact]
    public async Task AddWorkoutLiftOnHistoricalCreatedWorkoutUsesStandardInProgressGuard()
    {
        await using var dbContext = CreateDbContext();
        var liftId = Guid.NewGuid();
        SeedLift(dbContext, liftId, "Front Squat");
        await dbContext.SaveChangesAsync();

        var createHandler = new CreateHistoricalWorkoutCommandHandler(
            dbContext,
            new AcceptAllHistoricalWorkoutTimingValidator());

        var createResult = await createHandler.HandleAsync(new CreateHistoricalWorkoutCommand
        {
            TrainingDayLocalDate = new DateOnly(2026, 4, 20),
            StartTimeLocal = "09:30",
            SessionLengthMinutes = 60,
            Label = "Backfill Session",
        }, CancellationToken.None);

        Assert.Equal(CreateHistoricalWorkoutOutcome.Created, createResult.Outcome);
        Assert.Equal(WorkoutStatus.Completed, createResult.Workout!.Status);

        var addLiftHandler = new AddWorkoutLiftCommandHandler(dbContext);
        var addLiftAction = () => addLiftHandler.HandleAsync(new AddWorkoutLiftCommand
        {
            WorkoutId = createResult.Workout.Id,
            LiftId = liftId,
        }, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<WorkoutNotInProgressException>(addLiftAction);
        Assert.Equal(createResult.Workout.Id, exception.WorkoutId);
        Assert.Empty(await dbContext.WorkoutLiftEntries.ToListAsync());
    }

    [Fact]
    public async Task AddWorkoutSetOnHistoricalCreatedWorkoutReturnsConflictAndAvoidsWrite()
    {
        await using var dbContext = CreateDbContext();
        var liftId = Guid.NewGuid();
        SeedLift(dbContext, liftId, "Bench Press");
        await dbContext.SaveChangesAsync();

        var createHandler = new CreateHistoricalWorkoutCommandHandler(
            dbContext,
            new AcceptAllHistoricalWorkoutTimingValidator());

        var createResult = await createHandler.HandleAsync(new CreateHistoricalWorkoutCommand
        {
            TrainingDayLocalDate = new DateOnly(2026, 4, 20),
            StartTimeLocal = "07:15",
            SessionLengthMinutes = 45,
            Label = "Backfill Bench",
        }, CancellationToken.None);

        Assert.Equal(CreateHistoricalWorkoutOutcome.Created, createResult.Outcome);

        var workoutLiftEntryId = Guid.NewGuid();
        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = workoutLiftEntryId,
            WorkoutId = createResult.Workout!.Id,
            LiftId = liftId,
            DisplayName = "Bench Press",
            AddedAtUtc = new DateTime(2026, 4, 20, 7, 20, 0, DateTimeKind.Utc),
            Position = 1,
        });
        await dbContext.SaveChangesAsync();

        var addSetHandler = new AddWorkoutSetCommandHandler(dbContext);
        var addSetResult = await addSetHandler.HandleAsync(new AddWorkoutSetCommand
        {
            WorkoutId = createResult.Workout.Id,
            WorkoutLiftEntryId = workoutLiftEntryId,
            Reps = 8,
            Weight = 135m,
        }, CancellationToken.None);

        Assert.Equal(AddWorkoutSetOutcome.Conflict, addSetResult.Outcome);
        Assert.Contains("Workout must be in progress to add sets.", addSetResult.Errors["workout"]);
        Assert.Empty(await dbContext.WorkoutSets.ToListAsync());
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
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

    private sealed class AcceptAllHistoricalWorkoutTimingValidator : IHistoricalWorkoutTimingValidator
    {
        public Dictionary<string, string[]> Validate(
            DateOnly trainingDayLocalDate,
            string? startTimeLocal,
            int sessionLengthMinutes)
        {
            return [];
        }
    }
}
