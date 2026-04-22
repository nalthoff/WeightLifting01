namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class CompleteWorkoutResponse
{
    public required WorkoutSessionSummaryResponse Workout { get; init; }
}
