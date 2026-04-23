namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class UpdateWorkoutSetRequest
{
    public int Reps { get; init; }

    public decimal? Weight { get; init; }
}
