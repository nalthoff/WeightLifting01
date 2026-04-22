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
        DateTime? completedAtUtc,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        UserId = NormalizeUserId(userId);
        Status = status;
        Label = NormalizeLabel(label);
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        ValidateLifecycleState(status, completedAtUtc, startedAtUtc, updatedAtUtc);
    }

    public Guid Id { get; }

    public string UserId { get; }

    public WorkoutStatus Status { get; }

    public string? Label { get; }

    public DateTime StartedAtUtc { get; }

    public DateTime? CompletedAtUtc { get; }

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

    public Workout Complete(DateTime completedAtUtc)
    {
        if (Status != WorkoutStatus.InProgress)
        {
            throw new InvalidOperationException("Workout must be in progress to complete.");
        }

        var completedAtUtcNormalized = EnsureUtc(completedAtUtc, nameof(completedAtUtc));

        return new Workout(
            Id,
            UserId,
            WorkoutStatus.Completed,
            Label,
            StartedAtUtc,
            completedAtUtcNormalized,
            CreatedAtUtc,
            completedAtUtcNormalized);
    }

    private static void ValidateLifecycleState(
        WorkoutStatus status,
        DateTime? completedAtUtc,
        DateTime startedAtUtc,
        DateTime updatedAtUtc)
    {
        if (status == WorkoutStatus.InProgress && completedAtUtc is not null)
        {
            throw new ArgumentException("In-progress workout cannot have a completion timestamp.", nameof(completedAtUtc));
        }

        if (status == WorkoutStatus.Completed && completedAtUtc is null)
        {
            throw new ArgumentException("Completed workout must have a completion timestamp.", nameof(completedAtUtc));
        }

        if (completedAtUtc is not null && completedAtUtc.Value < startedAtUtc)
        {
            throw new ArgumentException("Completion timestamp cannot be earlier than start timestamp.", nameof(completedAtUtc));
        }

        if (updatedAtUtc < startedAtUtc)
        {
            throw new ArgumentException("Updated timestamp cannot be earlier than start timestamp.", nameof(updatedAtUtc));
        }
    }

    private static DateTime EnsureUtc(DateTime timestamp, string paramName)
    {
        if (timestamp.Kind == DateTimeKind.Utc)
        {
            return timestamp;
        }

        throw new ArgumentException("Timestamp must be in UTC.", paramName);
    }
}
