namespace WeightLifting.Api.Application.Lifts.Commands.DeactivateLift;

public sealed class DeactivateLiftCommand
{
    public required Guid LiftId { get; init; }
}
