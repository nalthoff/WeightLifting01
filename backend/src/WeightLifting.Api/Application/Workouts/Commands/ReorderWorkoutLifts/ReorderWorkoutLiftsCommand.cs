namespace WeightLifting.Api.Application.Workouts.Commands.ReorderWorkoutLifts;

public sealed class ReorderWorkoutLiftsCommand
{
    public required Guid WorkoutId { get; init; }

    public required IReadOnlyList<Guid> OrderedWorkoutLiftEntryIds { get; init; }
}
