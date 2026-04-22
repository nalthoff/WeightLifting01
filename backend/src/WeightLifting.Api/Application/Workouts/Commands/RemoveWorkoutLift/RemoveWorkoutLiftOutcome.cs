namespace WeightLifting.Api.Application.Workouts.Commands.RemoveWorkoutLift;

public enum RemoveWorkoutLiftOutcome
{
    Removed = 1,
    NotFound = 2,
    Conflict = 3,
}
