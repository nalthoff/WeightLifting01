namespace WeightLifting.Api.Application.Lifts.Commands.RenameLift;

public sealed class RenameLiftCommand
{
    public required Guid LiftId { get; init; }

    public required string Name { get; init; }
}
