using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Commands.WorkoutEntryMutability;

public static class WorkoutEntryMutabilityService
{
    public static async Task<WorkoutEntryMutabilityCheckResult> CheckAsync(
        WeightLiftingDbContext dbContext,
        Guid workoutId,
        string userId,
        IWorkoutEntryMutabilityRule mutabilityRule,
        CancellationToken cancellationToken)
    {
        var workout = await dbContext.Workouts.SingleOrDefaultAsync(
            item => item.Id == workoutId && item.UserId == userId,
            cancellationToken);

        if (workout is null)
        {
            return new WorkoutEntryMutabilityCheckResult
            {
                WorkoutExists = false,
                CanMutate = false,
            };
        }

        return new WorkoutEntryMutabilityCheckResult
        {
            WorkoutExists = true,
            CanMutate = mutabilityRule.CanMutate(workout.Status),
            Workout = workout,
        };
    }
}
