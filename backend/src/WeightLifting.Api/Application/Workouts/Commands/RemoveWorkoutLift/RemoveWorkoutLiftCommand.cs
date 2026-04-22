namespace WeightLifting.Api.Application.Workouts.Commands.RemoveWorkoutLift;

public sealed class RemoveWorkoutLiftCommand
{
    public required Guid WorkoutId { get; init; }

    public required Guid WorkoutLiftEntryId { get; init; }
}
