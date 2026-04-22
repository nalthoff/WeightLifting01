namespace WeightLifting.Api.Application.Workouts.Commands.RemoveWorkoutLift;

public sealed class RemoveWorkoutLiftResult
{
    public required RemoveWorkoutLiftOutcome Outcome { get; init; }

    public Guid WorkoutId { get; init; }

    public Guid WorkoutLiftEntryId { get; init; }
}
