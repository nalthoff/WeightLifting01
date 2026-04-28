namespace WeightLifting.Api.Api.Contracts.Workouts;

public sealed class CreateHistoricalWorkoutRequest
{
    public required DateOnly TrainingDayLocalDate { get; init; }

    public required string StartTimeLocal { get; init; }

    public required int SessionLengthMinutes { get; init; }

    public string? Label { get; init; }
}
