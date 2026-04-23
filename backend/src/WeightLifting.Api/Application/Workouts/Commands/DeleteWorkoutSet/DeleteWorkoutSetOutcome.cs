namespace WeightLifting.Api.Application.Workouts.Commands.DeleteWorkoutSet;

public enum DeleteWorkoutSetOutcome
{
    Deleted = 1,
    NotFound = 2,
    Conflict = 3,
}
