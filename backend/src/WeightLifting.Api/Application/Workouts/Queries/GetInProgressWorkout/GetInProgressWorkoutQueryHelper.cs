using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Queries.GetInProgressWorkout;

public sealed class GetInProgressWorkoutQueryHelper(WeightLiftingDbContext dbContext)
{
    public async Task<Workout?> GetAsync(string userId, CancellationToken cancellationToken)
    {
        var workoutEntity = await dbContext.Workouts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                workout => workout.UserId == userId && workout.Status == WorkoutStatus.InProgress,
                cancellationToken);

        if (workoutEntity is null)
        {
            return null;
        }

        return new Workout(
            workoutEntity.Id,
            workoutEntity.UserId,
            workoutEntity.Status,
            workoutEntity.Label,
            workoutEntity.StartedAtUtc,
            workoutEntity.CompletedAtUtc,
            workoutEntity.CreatedAtUtc,
            workoutEntity.UpdatedAtUtc);
    }
}
