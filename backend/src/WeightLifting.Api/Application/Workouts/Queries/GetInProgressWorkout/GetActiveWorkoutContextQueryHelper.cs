namespace WeightLifting.Api.Application.Workouts.Queries.GetInProgressWorkout;

public sealed class GetActiveWorkoutContextQueryHelper(GetInProgressWorkoutQueryHelper getInProgressWorkoutQueryHelper)
{
    public async Task<ActiveWorkoutContextState> GetAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var activeWorkout = await getInProgressWorkoutQueryHelper.GetAsync(userId, cancellationToken);
        return new ActiveWorkoutContextState(activeWorkout?.Id);
    }
}
