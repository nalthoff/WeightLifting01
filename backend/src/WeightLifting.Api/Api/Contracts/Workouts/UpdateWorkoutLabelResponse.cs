namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class UpdateWorkoutLabelResponse
{
    public required WorkoutSessionSummaryResponse Workout { get; init; }
}
