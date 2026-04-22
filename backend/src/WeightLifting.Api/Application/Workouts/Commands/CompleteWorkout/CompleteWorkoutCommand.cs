namespace WeightLifting.Api.Application.Workouts.Commands.CompleteWorkout;

public sealed class CompleteWorkoutCommand
{
    public required Guid WorkoutId { get; init; }
}
