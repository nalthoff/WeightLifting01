namespace WeightLifting.Api.Application.Workouts.Commands.AddWorkoutSet;

public sealed class AddWorkoutSetCommand
{
    public Guid WorkoutId { get; init; }

    public Guid WorkoutLiftEntryId { get; init; }

    public int Reps { get; init; }

    public decimal? Weight { get; init; }

    public bool AllowHistoricalEdits { get; init; }
}
