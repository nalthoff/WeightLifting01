namespace WeightLifting.Api.Application.Workouts.Commands.AddWorkoutLift;

public sealed class WorkoutNotInProgressException(Guid workoutId)
    : InvalidOperationException($"Workout '{workoutId}' is not in progress.")
{
    public Guid WorkoutId { get; } = workoutId;
}
