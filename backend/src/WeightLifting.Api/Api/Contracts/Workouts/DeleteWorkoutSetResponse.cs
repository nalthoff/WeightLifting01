namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class DeleteWorkoutSetResponse
{
    public required Guid WorkoutId { get; init; }

    public required Guid WorkoutLiftEntryId { get; init; }

    public required Guid SetId { get; init; }
}
