namespace WeightLifting.Api.Application.Workouts.Commands.DeleteWorkoutSet;

public sealed class DeleteWorkoutSetResult
{
    public required DeleteWorkoutSetOutcome Outcome { get; init; }

    public Guid WorkoutId { get; init; }

    public Guid WorkoutLiftEntryId { get; init; }

    public Guid SetId { get; init; }
}
