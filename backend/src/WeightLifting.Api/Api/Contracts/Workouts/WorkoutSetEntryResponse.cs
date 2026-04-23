namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class WorkoutSetEntryResponse
{
    public Guid Id { get; init; }

    public Guid WorkoutLiftEntryId { get; init; }

    public int SetNumber { get; init; }

    public int Reps { get; init; }

    public decimal? Weight { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime UpdatedAtUtc { get; init; }
}
