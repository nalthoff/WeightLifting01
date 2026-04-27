namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class InlineLiftHistorySetResponse
{
    public required int SetNumber { get; init; }

    public required int Reps { get; init; }

    public decimal? Weight { get; init; }
}
