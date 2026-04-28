namespace WeightLifting.Api.Application.Workouts.Commands.CreateHistoricalWorkout;

public sealed class CreateHistoricalWorkoutCommand
{
    public required DateOnly TrainingDayLocalDate { get; init; }

    public required string StartTimeLocal { get; init; }

    public required int SessionLengthMinutes { get; init; }

    public string? Label { get; init; }
}
