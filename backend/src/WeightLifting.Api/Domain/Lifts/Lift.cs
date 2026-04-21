namespace WeightLifting.Api.Domain.Lifts;

public sealed class Lift
{
    public Lift(Guid id, string name, bool isActive, DateTime createdAtUtc)
    {
        Id = id;
        Name = NormalizeName(name);
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }

    public string Name { get; }

    public bool IsActive { get; }

    public DateTime CreatedAtUtc { get; }

    public Lift Rename(string name)
    {
        var normalizedName = NormalizeName(name);

        if (string.Equals(normalizedName, Name, StringComparison.Ordinal))
        {
            return this;
        }

        return new Lift(Id, normalizedName, IsActive, CreatedAtUtc);
    }

    public static string NormalizeName(string name)
    {
        var normalizedName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException("Lift name is required.", nameof(name));
        }

        return normalizedName;
    }
}
