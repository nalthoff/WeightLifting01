namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class WorkoutHistoryItemResponse
{
    public required Guid WorkoutId { get; init; }

    public required string Label { get; init; }

    public required DateTime CompletedAtUtc { get; init; }

    public required string DurationDisplay { get; init; }

    public required int LiftCount { get; init; }
}
