namespace WeightLifting.Api.Application.Workouts.Commands.DeleteWorkoutSet;

public sealed class DeleteWorkoutSetCommand
{
    public required Guid WorkoutId { get; init; }

    public required Guid WorkoutLiftEntryId { get; init; }

    public required Guid SetId { get; init; }
}
