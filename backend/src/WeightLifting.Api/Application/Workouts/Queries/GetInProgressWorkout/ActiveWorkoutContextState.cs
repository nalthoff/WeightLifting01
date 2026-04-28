namespace WeightLifting.Api.Application.Workouts.Queries.GetInProgressWorkout;

public sealed record ActiveWorkoutContextState(Guid? ActiveWorkoutId)
{
    public bool HasActiveWorkout => ActiveWorkoutId.HasValue;

    public bool CanCoexistWithHistoricalWorkout(Guid historicalWorkoutId)
    {
        return ActiveWorkoutId != historicalWorkoutId;
    }

    public ActiveWorkoutContextState WithActiveWorkout(Guid? activeWorkoutId)
    {
        return this with
        {
            ActiveWorkoutId = activeWorkoutId,
        };
    }
}
