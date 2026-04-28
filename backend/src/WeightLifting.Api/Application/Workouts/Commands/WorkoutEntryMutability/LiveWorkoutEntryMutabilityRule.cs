using WeightLifting.Api.Domain.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.WorkoutEntryMutability;

public sealed class LiveWorkoutEntryMutabilityRule : IWorkoutEntryMutabilityRule
{
    public bool CanMutate(WorkoutStatus status) => status == WorkoutStatus.InProgress;
}
