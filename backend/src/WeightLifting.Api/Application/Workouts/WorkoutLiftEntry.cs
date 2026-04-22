namespace WeightLifting.Api.Application.Workouts;

public sealed class WorkoutLiftEntry
{
    public Guid Id { get; init; }

    public Guid WorkoutId { get; init; }

    public Guid LiftId { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public DateTime AddedAtUtc { get; init; }

    public int Position { get; init; }
}
