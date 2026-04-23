namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class WorkoutHistoryResponse
{
    public required IReadOnlyList<WorkoutHistoryItemResponse> Items { get; init; }
}
