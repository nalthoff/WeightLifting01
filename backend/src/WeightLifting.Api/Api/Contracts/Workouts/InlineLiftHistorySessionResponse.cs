namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class InlineLiftHistorySessionResponse
{
    public required Guid WorkoutId { get; init; }

    public string? WorkoutLabel { get; init; }

    public required DateTime CompletedAtUtc { get; init; }

    public required IReadOnlyList<InlineLiftHistorySetResponse> Sets { get; init; }
}
