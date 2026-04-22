namespace WeightLifting.Api.Application.Workouts.Commands.AddWorkoutLift;

public sealed class LiftNotActiveException(Guid liftId)
    : InvalidOperationException($"Lift '{liftId}' is inactive.")
{
    public Guid LiftId { get; } = liftId;
}
