namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class StartWorkoutCreatedResponse
{
    public required WorkoutSessionSummaryResponse Workout { get; init; }
}
