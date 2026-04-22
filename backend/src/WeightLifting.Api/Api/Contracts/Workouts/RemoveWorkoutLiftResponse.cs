namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class RemoveWorkoutLiftResponse
{
    public required Guid WorkoutId { get; init; }

    public required Guid WorkoutLiftEntryId { get; init; }
}
