namespace WeightLifting.Api.Infrastructure.Persistence.Entities;

public sealed class WorkoutSetEntity
{
    public Guid Id { get; set; }

    public Guid WorkoutId { get; set; }

    public Guid WorkoutLiftEntryId { get; set; }

    public int SetNumber { get; set; }

    public int Reps { get; set; }

    public decimal? Weight { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
