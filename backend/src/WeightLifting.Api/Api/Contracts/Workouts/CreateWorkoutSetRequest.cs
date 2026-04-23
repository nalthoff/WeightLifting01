namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class CreateWorkoutSetRequest
{
    public int Reps { get; init; }

    public decimal? Weight { get; init; }
}
