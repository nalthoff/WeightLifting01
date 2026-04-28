using WeightLifting.Api.Domain.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.WorkoutEntryMutability;

public interface IWorkoutEntryMutabilityRule
{
    bool CanMutate(WorkoutStatus status);
}
