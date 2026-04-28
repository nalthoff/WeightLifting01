using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.CreateHistoricalWorkout;
using WeightLifting.Api.Application.Workouts.Commands.TimingValidation;
using WeightLifting.Api.Application.Workouts.Queries.GetInProgressWorkout;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts;

public sealed class HistoricalWorkoutActiveContextTests
{
    private const string DefaultUserId = "default-user";

    [Fact]
    public async Task CreateHistoricalWorkoutDoesNotChangeExistingActiveWorkoutContext()
    {
        await using var dbContext = CreateDbContext();
        var activeWorkoutId = Guid.NewGuid();
        var activeWorkoutStartedAtUtc = new DateTime(2026, 4, 25, 13, 0, 0, DateTimeKind.Utc);

        SeedInProgressWorkout(dbContext, activeWorkoutId, activeWorkoutStartedAtUtc);
        await dbContext.SaveChangesAsync();

        var activeContextHelper = new GetActiveWorkoutContextQueryHelper(new GetInProgressWorkoutQueryHelper(dbContext));
        var beforeCreateState = await activeContextHelper.GetAsync(DefaultUserId, CancellationToken.None);

        var createHistoricalHandler = new CreateHistoricalWorkoutCommandHandler(
            dbContext,
            new AcceptAllHistoricalWorkoutTimingValidator());

        var historicalResult = await createHistoricalHandler.HandleAsync(new CreateHistoricalWorkoutCommand
        {
            TrainingDayLocalDate = new DateOnly(2026, 4, 20),
            StartTimeLocal = "07:30",
            SessionLengthMinutes = 45,
            Label = "Backfilled Session",
        }, CancellationToken.None);

        var afterCreateState = await activeContextHelper.GetAsync(DefaultUserId, CancellationToken.None);

        Assert.True(beforeCreateState.HasActiveWorkout);
        Assert.Equal(activeWorkoutId, beforeCreateState.ActiveWorkoutId);
        Assert.Equal(CreateHistoricalWorkoutOutcome.Created, historicalResult.Outcome);
        Assert.NotNull(historicalResult.Workout);
        Assert.Equal(WorkoutStatus.Completed, historicalResult.Workout!.Status);

        Assert.True(afterCreateState.HasActiveWorkout);
        Assert.Equal(activeWorkoutId, afterCreateState.ActiveWorkoutId);
        Assert.NotEqual(activeWorkoutId, historicalResult.Workout.Id);

        var persistedActiveWorkout = await dbContext.Workouts.SingleAsync(item => item.Id == activeWorkoutId);
        Assert.Equal(WorkoutStatus.InProgress, persistedActiveWorkout.Status);
        Assert.Equal(activeWorkoutStartedAtUtc, persistedActiveWorkout.StartedAtUtc);
        Assert.Null(persistedActiveWorkout.CompletedAtUtc);
    }

    [Fact]
    public void ActiveWorkoutContextStateCoexistenceGuardAllowsDistinctHistoricalWorkout()
    {
        var activeWorkoutId = Guid.NewGuid();
        var historicalWorkoutId = Guid.NewGuid();
        var state = new ActiveWorkoutContextState(activeWorkoutId);

        var canCoexist = state.CanCoexistWithHistoricalWorkout(historicalWorkoutId);

        Assert.True(state.HasActiveWorkout);
        Assert.True(canCoexist);
    }

    [Fact]
    public void ActiveWorkoutContextStateCoexistenceGuardRejectsSameWorkoutId()
    {
        var workoutId = Guid.NewGuid();
        var state = new ActiveWorkoutContextState(workoutId);

        var canCoexist = state.CanCoexistWithHistoricalWorkout(workoutId);

        Assert.False(canCoexist);
    }

    [Fact]
    public void ActiveWorkoutContextStateWithActiveWorkoutReturnsUpdatedImmutableState()
    {
        var originalState = new ActiveWorkoutContextState(null);
        var workoutId = Guid.NewGuid();

        var updatedState = originalState.WithActiveWorkout(workoutId);

        Assert.False(originalState.HasActiveWorkout);
        Assert.Null(originalState.ActiveWorkoutId);
        Assert.True(updatedState.HasActiveWorkout);
        Assert.Equal(workoutId, updatedState.ActiveWorkoutId);
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }

    private static void SeedInProgressWorkout(
        WeightLiftingDbContext dbContext,
        Guid workoutId,
        DateTime startedAtUtc)
    {
        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workoutId,
            UserId = DefaultUserId,
            Status = WorkoutStatus.InProgress,
            Label = "Current Session",
            StartedAtUtc = startedAtUtc,
            CreatedAtUtc = startedAtUtc,
            UpdatedAtUtc = startedAtUtc,
            CompletedAtUtc = null,
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
