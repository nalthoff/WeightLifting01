namespace WeightLifting.Api.Application.Lifts.Commands.CreateLift;

public sealed class CreateLiftCommand
{
    public required string Name { get; init; }

    public string? ClientRequestId { get; init; }
}
