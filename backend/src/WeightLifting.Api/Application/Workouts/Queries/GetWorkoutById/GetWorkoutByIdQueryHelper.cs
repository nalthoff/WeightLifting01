using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Queries.GetWorkoutById;

public sealed class GetWorkoutByIdQueryHelper(WeightLiftingDbContext dbContext)
{
    public async Task<Workout?> GetAsync(
        Guid workoutId,
        string userId,
        CancellationToken cancellationToken,
        bool requireCompleted = false)
    {
        var workoutEntity = await dbContext.Workouts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                workout => workout.Id == workoutId && workout.UserId == userId,
                cancellationToken);

        if (workoutEntity is null)
        {
            return null;
        }

        if (requireCompleted && workoutEntity.Status != WorkoutStatus.Completed)
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
