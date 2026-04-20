namespace WeightLifting.Api.Api.Contracts.Lifts;

public sealed class LiftListItemResponse
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required bool IsActive { get; init; }
}
