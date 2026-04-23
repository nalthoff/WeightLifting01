namespace WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutSet;

public sealed class UpdateWorkoutSetResult
{
    public required UpdateWorkoutSetOutcome Outcome { get; init; }

    public WorkoutSetEntry? Set { get; init; }

    public IReadOnlyDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
}
