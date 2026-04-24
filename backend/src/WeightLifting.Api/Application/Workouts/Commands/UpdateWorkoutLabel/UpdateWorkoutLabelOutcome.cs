namespace WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutLabel;

public enum UpdateWorkoutLabelOutcome
{
    Updated = 0,
    NotFound = 1,
    Conflict = 2,
    ValidationFailed = 3,
}
