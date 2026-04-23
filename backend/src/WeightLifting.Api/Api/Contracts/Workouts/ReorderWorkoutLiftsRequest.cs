namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class ReorderWorkoutLiftsRequest
{
    public IReadOnlyList<Guid> OrderedWorkoutLiftEntryIds { get; init; } = [];
}
