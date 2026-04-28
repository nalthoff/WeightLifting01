using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Queries.GetInProgressWorkout;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.CompleteWorkout;

public sealed class CompleteWorkoutCommandHandler(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";
    private static readonly IWorkoutCompletionLifecycleRule LiveLifecycleRule = new LiveWorkoutCompletionLifecycleRule();
    private static readonly IWorkoutCompletionLifecycleRule HistoricalLifecycleRule = new HistoricalWorkoutCompletionLifecycleRule();

    public async Task<CompleteWorkoutResult> HandleAsync(
        CompleteWorkoutCommand command,
        CancellationToken cancellationToken)
    {
        var workoutEntity = await dbContext.Workouts
            .SingleOrDefaultAsync(
                workout => workout.Id == command.WorkoutId && workout.UserId == DefaultUserId,
                cancellationToken);

        if (workoutEntity is null)
        {
            return new CompleteWorkoutResult
            {
                Outcome = CompleteWorkoutOutcome.NotFound,
            };
        }

        var activeWorkoutId = await dbContext.Workouts
            .AsNoTracking()
            .Where(workout =>
                workout.UserId == DefaultUserId &&
                workout.Status == WorkoutStatus.InProgress &&
                workout.Id != command.WorkoutId)
            .Select(workout => workout.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var activeWorkoutContext = activeWorkoutId == Guid.Empty
            ? new ActiveWorkoutContextState(null)
            : new ActiveWorkoutContextState(activeWorkoutId);

        var lifecycleRule = activeWorkoutContext.HasActiveWorkout
            ? HistoricalLifecycleRule
            : LiveLifecycleRule;

        var lifecycleResult = WorkoutCompletionLifecycleService.Check(
            workoutEntity,
            command.WorkoutId,
            activeWorkoutContext,
            lifecycleRule);

        if (!lifecycleResult.CanComplete)
        {
            return new CompleteWorkoutResult
            {
                Outcome = CompleteWorkoutOutcome.Conflict,
                Errors = lifecycleResult.Errors,
            };
        }

        var completionTimestampUtc = command.CompletedAtUtc ?? DateTime.UtcNow;

        var completedWorkout = new Workout(
            workoutEntity.Id,
            workoutEntity.UserId,
            workoutEntity.Status,
            workoutEntity.Label,
            workoutEntity.StartedAtUtc,
            workoutEntity.CompletedAtUtc,
            workoutEntity.CreatedAtUtc,
            workoutEntity.UpdatedAtUtc)
            .Complete(completionTimestampUtc);

        workoutEntity.Status = completedWorkout.Status;
        workoutEntity.CompletedAtUtc = completedWorkout.CompletedAtUtc;
        workoutEntity.UpdatedAtUtc = completedWorkout.UpdatedAtUtc;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CompleteWorkoutResult
        {
            Outcome = CompleteWorkoutOutcome.Completed,
            Workout = completedWorkout,
        };
    }
}

internal interface IWorkoutCompletionLifecycleRule
{
    bool CanCoexistWithActiveWorkout(ActiveWorkoutContextState activeWorkoutContextState, Guid workoutId);
}

internal abstract class WorkoutCompletionLifecycleRuleBase : IWorkoutCompletionLifecycleRule
{
    public abstract bool CanCoexistWithActiveWorkout(ActiveWorkoutContextState activeWorkoutContextState, Guid workoutId);
}

internal sealed class LiveWorkoutCompletionLifecycleRule : WorkoutCompletionLifecycleRuleBase
{
    public override bool CanCoexistWithActiveWorkout(ActiveWorkoutContextState activeWorkoutContextState, Guid workoutId)
    {
        if (!activeWorkoutContextState.HasActiveWorkout)
        {
            return true;
        }

        return activeWorkoutContextState.ActiveWorkoutId == workoutId;
    }
}

internal sealed class HistoricalWorkoutCompletionLifecycleRule : WorkoutCompletionLifecycleRuleBase
{
    public override bool CanCoexistWithActiveWorkout(ActiveWorkoutContextState activeWorkoutContextState, Guid workoutId)
    {
        return activeWorkoutContextState.CanCoexistWithHistoricalWorkout(workoutId);
    }
}

internal static class WorkoutCompletionLifecycleService
{
    public static WorkoutCompletionLifecycleCheckResult Check(
        WorkoutEntity workout,
        Guid workoutId,
        ActiveWorkoutContextState activeWorkoutContextState,
        IWorkoutCompletionLifecycleRule lifecycleRule)
    {
        if (workout.Status != WorkoutStatus.InProgress)
        {
            return new WorkoutCompletionLifecycleCheckResult
            {
                CanComplete = false,
                Errors = new Dictionary<string, string[]>
                {
                    ["workout"] = ["Workout must be in progress to complete."],
                },
            };
        }

        if (!lifecycleRule.CanCoexistWithActiveWorkout(activeWorkoutContextState, workoutId))
        {
            return new WorkoutCompletionLifecycleCheckResult
            {
                CanComplete = false,
                Errors = new Dictionary<string, string[]>
                {
                    ["workout"] = ["Workout cannot be completed from the current lifecycle context."],
                },
            };
        }

        return new WorkoutCompletionLifecycleCheckResult
        {
            CanComplete = true,
        };
    }
}

internal sealed class WorkoutCompletionLifecycleCheckResult
{
    public required bool CanComplete { get; init; }

    public IReadOnlyDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
}
