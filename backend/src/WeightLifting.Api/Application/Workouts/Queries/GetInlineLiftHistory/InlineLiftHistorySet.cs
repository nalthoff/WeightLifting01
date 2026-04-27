namespace WeightLifting.Api.Application.Workouts.Queries.GetInlineLiftHistory;

public sealed class InlineLiftHistorySet
{
    public required int SetNumber { get; init; }

    public required int Reps { get; init; }

    public decimal? Weight { get; init; }
}
