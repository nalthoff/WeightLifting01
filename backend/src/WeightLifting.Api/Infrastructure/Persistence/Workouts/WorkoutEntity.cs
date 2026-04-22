using WeightLifting.Api.Domain.Workouts;

namespace WeightLifting.Api.Infrastructure.Persistence.Workouts;

public sealed class WorkoutEntity
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public WorkoutStatus Status { get; set; } = WorkoutStatus.InProgress;

    public string? Label { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
