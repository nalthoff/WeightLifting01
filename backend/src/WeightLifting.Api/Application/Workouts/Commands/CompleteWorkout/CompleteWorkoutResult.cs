using WeightLifting.Api.Domain.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.CompleteWorkout;

public sealed class CompleteWorkoutResult
{
    public required CompleteWorkoutOutcome Outcome { get; init; }

    public Workout? Workout { get; init; }
}
