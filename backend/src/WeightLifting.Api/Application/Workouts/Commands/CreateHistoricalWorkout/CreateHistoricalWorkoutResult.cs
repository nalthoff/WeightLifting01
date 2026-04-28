using WeightLifting.Api.Domain.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.CreateHistoricalWorkout;

public sealed class CreateHistoricalWorkoutResult
{
    public required CreateHistoricalWorkoutOutcome Outcome { get; init; }

    public Workout? Workout { get; init; }

    public Dictionary<string, string[]> Errors { get; init; } = [];
}
