namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class ActiveWorkoutSummaryResponse
{
    public required WorkoutSessionSummaryResponse Workout { get; init; }
}
