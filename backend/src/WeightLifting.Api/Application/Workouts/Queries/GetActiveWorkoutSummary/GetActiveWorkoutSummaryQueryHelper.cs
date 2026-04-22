using WeightLifting.Api.Domain.Workouts;

namespace WeightLifting.Api.Application.Workouts.Queries.GetActiveWorkoutSummary;

public sealed class GetActiveWorkoutSummaryQueryHelper(GetInProgressWorkout.GetInProgressWorkoutQueryHelper getInProgressWorkoutQueryHelper)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public Task<Workout?> GetAsync(CancellationToken cancellationToken)
    {
        return getInProgressWorkoutQueryHelper.GetAsync(DefaultUserId, cancellationToken);
    }
}
