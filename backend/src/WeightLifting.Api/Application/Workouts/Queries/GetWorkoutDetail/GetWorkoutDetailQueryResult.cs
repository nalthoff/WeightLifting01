using WeightLifting.Api.Domain.Workouts;

namespace WeightLifting.Api.Application.Workouts.Queries.GetWorkoutDetail;

public sealed class GetWorkoutDetailQueryResult
{
    public required Workout Workout { get; init; }

    public required IReadOnlyList<WorkoutLiftEntry> Lifts { get; init; }
}
