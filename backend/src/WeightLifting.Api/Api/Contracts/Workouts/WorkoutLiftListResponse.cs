namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class WorkoutLiftListResponse
{
    public required IReadOnlyList<WorkoutLiftEntryResponse> Items { get; init; }
}
