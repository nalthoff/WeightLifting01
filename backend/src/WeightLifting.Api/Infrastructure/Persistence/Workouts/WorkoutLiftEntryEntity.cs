namespace WeightLifting.Api.Infrastructure.Persistence.Workouts;

public sealed class WorkoutLiftEntryEntity
{
    public Guid Id { get; set; }

    public Guid WorkoutId { get; set; }

    public Guid LiftId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public DateTime AddedAtUtc { get; set; }

    public int Position { get; set; }
}
