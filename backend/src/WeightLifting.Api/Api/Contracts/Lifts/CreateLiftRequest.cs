namespace WeightLifting.Api.Api.Contracts.Lifts;

public sealed class CreateLiftRequest
{
    public required string Name { get; init; }

    public string? ClientRequestId { get; init; }
}
