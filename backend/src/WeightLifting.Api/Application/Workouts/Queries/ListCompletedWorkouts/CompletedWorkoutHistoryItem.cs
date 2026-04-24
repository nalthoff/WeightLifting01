namespace WeightLifting.Api.Application.Workouts.Queries.ListCompletedWorkouts;

public sealed class CompletedWorkoutHistoryItem
{
    public required Guid WorkoutId { get; init; }

    public string? Label { get; init; }

    public required DateTime CompletedAtUtc { get; init; }

    public required string DurationDisplay { get; init; }

    public required int LiftCount { get; init; }
}
