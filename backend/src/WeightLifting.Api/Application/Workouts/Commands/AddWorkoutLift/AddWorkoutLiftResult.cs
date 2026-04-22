using WeightLifting.Api.Application.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.AddWorkoutLift;

public sealed class AddWorkoutLiftResult
{
    public required WorkoutLiftEntry WorkoutLift { get; init; }
}
