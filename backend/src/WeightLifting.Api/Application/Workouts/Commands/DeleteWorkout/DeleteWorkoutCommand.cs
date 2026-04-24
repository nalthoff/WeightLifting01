namespace WeightLifting.Api.Application.Workouts.Commands.DeleteWorkout;

public sealed class DeleteWorkoutCommand
{
    public required Guid WorkoutId { get; init; }
}
