namespace WeightLifting.Api.Application.Lifts.Queries.GetLifts;

public sealed class GetLiftsQuery
{
    public bool ActiveOnly { get; init; } = true;
}
