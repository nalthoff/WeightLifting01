namespace WeightLifting.Api.Application.Workouts.Commands.ReorderWorkoutLifts;

public enum ReorderWorkoutLiftsOutcome
{
    Reordered = 1,
    NotFound = 2,
    Conflict = 3,
    ValidationFailed = 4,
}
