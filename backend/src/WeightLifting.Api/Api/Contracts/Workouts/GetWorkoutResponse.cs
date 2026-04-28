namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class GetWorkoutResponse
{
    public required WorkoutSessionSummaryResponse Workout { get; init; }

    public required IReadOnlyList<WorkoutLiftEntryResponse> Lifts { get; init; }
}
