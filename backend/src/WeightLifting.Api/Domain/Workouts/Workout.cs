namespace WeightLifting.Api.Domain.Workouts;

public sealed class Workout
{
    public const int MaxLabelLength = 200;

    public Workout(
        Guid id,
        string userId,
        WorkoutStatus status,
        string? label,
        DateTime startedAtUtc,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        UserId = NormalizeUserId(userId);
        Status = status;
        Label = NormalizeLabel(label);
        StartedAtUtc = startedAtUtc;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; }

    public string UserId { get; }

    public WorkoutStatus Status { get; }

    public string? Label { get; }

    public DateTime StartedAtUtc { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; }

    public static string NormalizeUserId(string userId)
    {
        var normalizedUserId = userId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedUserId))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        return normalizedUserId;
    }

    public static string? NormalizeLabel(string? label)
    {
        if (label is null)
        {
            return null;
        }

        var normalizedLabel = label.Trim();
        if (normalizedLabel.Length == 0)
        {
            return null;
        }

        if (normalizedLabel.Length > MaxLabelLength)
        {
            throw new ArgumentException($"Workout label must be {MaxLabelLength} characters or fewer.", nameof(label));
        }

        return normalizedLabel;
    }
}
