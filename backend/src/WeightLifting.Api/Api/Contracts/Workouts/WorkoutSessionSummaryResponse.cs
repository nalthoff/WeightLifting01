namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class WorkoutSessionSummaryResponse
{
    public required Guid Id { get; init; }

    public required string Status { get; init; }

    public string? Label { get; init; }

    public required DateTime StartedAtUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }
}
