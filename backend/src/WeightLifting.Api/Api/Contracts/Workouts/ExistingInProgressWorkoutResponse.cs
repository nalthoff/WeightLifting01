namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class ExistingInProgressWorkoutResponse
{
    public required string Title { get; init; }

    public required int Status { get; init; }

    public required WorkoutSessionSummaryResponse Workout { get; init; }
}
