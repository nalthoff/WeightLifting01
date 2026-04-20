namespace WeightLifting.Api.Api.Contracts.Lifts;

public sealed class LiftResponse
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required bool IsActive { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
