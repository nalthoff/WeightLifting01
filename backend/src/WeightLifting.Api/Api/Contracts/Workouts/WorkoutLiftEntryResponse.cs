namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class WorkoutLiftEntryResponse
{
    public Guid Id { get; init; }

    public Guid WorkoutId { get; init; }

    public Guid LiftId { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public DateTime AddedAtUtc { get; init; }

    public int Position { get; init; }
}
