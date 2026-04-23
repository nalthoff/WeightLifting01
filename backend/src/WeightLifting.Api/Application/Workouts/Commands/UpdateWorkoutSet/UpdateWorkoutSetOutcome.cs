namespace WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutSet;

public enum UpdateWorkoutSetOutcome
{
    Updated = 1,
    NotFound = 2,
    Conflict = 3,
    ValidationFailed = 4,
}
