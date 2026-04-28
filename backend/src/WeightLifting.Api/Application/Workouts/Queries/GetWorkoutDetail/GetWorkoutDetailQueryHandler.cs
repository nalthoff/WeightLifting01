using WeightLifting.Api.Application.Workouts.Queries.GetWorkoutById;
using WeightLifting.Api.Application.Workouts.Queries.ListWorkoutLifts;

namespace WeightLifting.Api.Application.Workouts.Queries.GetWorkoutDetail;

public sealed class GetWorkoutDetailQueryHandler(
    GetWorkoutByIdQueryHelper getWorkoutByIdQueryHelper,
    ListWorkoutLiftsQueryHelper listWorkoutLiftsQueryHelper)
{
    public async Task<GetWorkoutDetailQueryResult?> HandleAsync(
        Guid workoutId,
        string userId,
        bool requireCompleted,
        CancellationToken cancellationToken)
    {
        var workout = await getWorkoutByIdQueryHelper.GetAsync(
            workoutId,
            userId,
            cancellationToken,
            requireCompleted);
        if (workout is null)
        {
            return null;
        }

        var lifts = await listWorkoutLiftsQueryHelper.GetAsync(
            workoutId,
            cancellationToken,
            requireCompleted);

        return new GetWorkoutDetailQueryResult
        {
            Workout = workout,
            Lifts = lifts,
        };
    }
}
