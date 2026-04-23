namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class UpdateWorkoutSetResponse
{
    public Guid WorkoutId { get; init; }

    public Guid WorkoutLiftEntryId { get; init; }

    public required WorkoutSetEntryResponse Set { get; init; }
}
