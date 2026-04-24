namespace WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutLabel;

public sealed class UpdateWorkoutLabelCommand
{
    public Guid WorkoutId { get; init; }

    public string? Label { get; init; }
}
