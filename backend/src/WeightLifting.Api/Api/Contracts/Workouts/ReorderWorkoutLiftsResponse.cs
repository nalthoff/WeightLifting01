namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class ReorderWorkoutLiftsResponse
{
    public required Guid WorkoutId { get; init; }

    public required IReadOnlyList<WorkoutLiftEntryResponse> Items { get; init; }
}
