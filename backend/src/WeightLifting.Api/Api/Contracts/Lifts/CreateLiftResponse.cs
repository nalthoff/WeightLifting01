namespace WeightLifting.Api.Api.Contracts.Lifts;

public sealed class CreateLiftResponse
{
    public required LiftResponse Lift { get; init; }
}
