using WeightLifting.Api.Domain.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutLabel;

public sealed class UpdateWorkoutLabelResult
{
    public required UpdateWorkoutLabelOutcome Outcome { get; init; }

    public Workout? Workout { get; init; }

    public Dictionary<string, string[]> Errors { get; init; } = new();
}
