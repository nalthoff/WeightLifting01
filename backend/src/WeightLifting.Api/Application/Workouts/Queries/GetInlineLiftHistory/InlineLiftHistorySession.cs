namespace WeightLifting.Api.Application.Workouts.Queries.GetInlineLiftHistory;

public sealed class InlineLiftHistorySession
{
    public required Guid WorkoutId { get; init; }

    public string? WorkoutLabel { get; init; }

    public required DateTime CompletedAtUtc { get; init; }

    public required IReadOnlyList<InlineLiftHistorySet> Sets { get; init; }
}
