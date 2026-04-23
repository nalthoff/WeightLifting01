using WeightLifting.Api.Application.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.ReorderWorkoutLifts;

public sealed class ReorderWorkoutLiftsResult
{
    public required ReorderWorkoutLiftsOutcome Outcome { get; init; }

    public Guid WorkoutId { get; init; }

    public IReadOnlyList<WorkoutLiftEntry> Items { get; init; } = [];
}
