namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class AddWorkoutLiftResponse
{
    public required WorkoutLiftEntryResponse WorkoutLift { get; init; }
}
