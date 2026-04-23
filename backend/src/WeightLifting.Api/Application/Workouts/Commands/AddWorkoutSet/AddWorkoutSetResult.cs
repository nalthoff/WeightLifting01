namespace WeightLifting.Api.Application.Workouts.Commands.AddWorkoutSet;

public sealed class AddWorkoutSetResult
{
    public required AddWorkoutSetOutcome Outcome { get; init; }

    public WorkoutSetEntry? Set { get; init; }

    public IReadOnlyDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
}
