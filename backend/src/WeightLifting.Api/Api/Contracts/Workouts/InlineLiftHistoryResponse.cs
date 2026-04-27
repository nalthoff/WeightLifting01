namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class InlineLiftHistoryResponse
{
    public required Guid WorkoutId { get; init; }

    public required Guid WorkoutLiftEntryId { get; init; }

    public required IReadOnlyList<InlineLiftHistorySessionResponse> Items { get; init; }
}
