namespace WeightLifting.Api.Application.Lifts.Commands.RenameLift;

public sealed class DuplicateLiftNameException(string normalizedName)
    : InvalidOperationException($"Lift name '{normalizedName}' already exists.")
{
    public string NormalizedName { get; } = normalizedName;
}
