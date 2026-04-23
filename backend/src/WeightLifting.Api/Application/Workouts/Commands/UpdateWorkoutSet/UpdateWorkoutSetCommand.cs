namespace WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutSet;

public sealed class UpdateWorkoutSetCommand
{
    public Guid WorkoutId { get; init; }

    public Guid WorkoutLiftEntryId { get; init; }

    public Guid SetId { get; init; }

    public int Reps { get; init; }

    public decimal? Weight { get; init; }
}
