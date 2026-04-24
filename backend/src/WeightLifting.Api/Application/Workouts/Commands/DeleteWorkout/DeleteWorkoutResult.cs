namespace WeightLifting.Api.Application.Workouts.Commands.DeleteWorkout;

public sealed class DeleteWorkoutResult
{
    public required DeleteWorkoutOutcome Outcome { get; init; }

    public Guid WorkoutId { get; init; }
}
