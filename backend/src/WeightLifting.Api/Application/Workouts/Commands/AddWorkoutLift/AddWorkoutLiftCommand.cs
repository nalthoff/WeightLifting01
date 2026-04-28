namespace WeightLifting.Api.Application.Workouts.Commands.AddWorkoutLift;

public sealed class AddWorkoutLiftCommand
{
    public Guid WorkoutId { get; init; }

    public Guid LiftId { get; init; }

    public bool AllowHistoricalEdits { get; init; }
}
