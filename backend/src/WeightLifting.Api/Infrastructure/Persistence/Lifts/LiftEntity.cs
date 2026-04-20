namespace WeightLifting.Api.Infrastructure.Persistence.Lifts;

public sealed class LiftEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
}
