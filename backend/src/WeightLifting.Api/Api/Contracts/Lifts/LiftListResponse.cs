namespace WeightLifting.Api.Api.Contracts.Lifts;

public sealed class LiftListResponse
{
    public required IReadOnlyList<LiftListItemResponse> Items { get; init; }

    public DateTime? LastSyncedAtUtc { get; init; }
}
