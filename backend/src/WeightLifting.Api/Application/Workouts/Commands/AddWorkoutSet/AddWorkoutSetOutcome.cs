namespace WeightLifting.Api.Application.Workouts.Commands.AddWorkoutSet;

public enum AddWorkoutSetOutcome
{
    Created = 1,
    NotFound = 2,
    Conflict = 3,
    ValidationFailed = 4,
}
