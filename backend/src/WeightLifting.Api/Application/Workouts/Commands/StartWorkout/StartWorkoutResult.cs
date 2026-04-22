using WeightLifting.Api.Domain.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.StartWorkout;

public sealed class StartWorkoutResult
{
    public required StartWorkoutOutcome Outcome { get; init; }

    public required Workout Workout { get; init; }
}
