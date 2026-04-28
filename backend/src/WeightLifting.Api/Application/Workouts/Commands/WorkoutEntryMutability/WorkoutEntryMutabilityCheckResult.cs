using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.WorkoutEntryMutability;

public sealed class WorkoutEntryMutabilityCheckResult
{
    public required bool WorkoutExists { get; init; }

    public required bool CanMutate { get; init; }

    public WorkoutEntity? Workout { get; init; }
}
