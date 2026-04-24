namespace WeightLifting.Api.Application.Workouts.Commands.DeleteWorkout;

public enum DeleteWorkoutOutcome
{
    Deleted = 1,
    NotFound = 2,
    Conflict = 3,
}
