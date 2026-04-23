using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Queries.ListCompletedWorkouts;

public sealed class ListCompletedWorkoutsQueryHelper(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<IReadOnlyList<CompletedWorkoutHistoryItem>> GetAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Workouts
            .AsNoTracking()
            .Where(workout =>
                workout.UserId == DefaultUserId
                && workout.Status == WorkoutStatus.Completed
                && workout.CompletedAtUtc.HasValue)
            .OrderByDescending(workout => workout.CompletedAtUtc)
            .Select(workout => new CompletedWorkoutHistoryItem
            {
                WorkoutId = workout.Id,
                Label = workout.Label,
                CompletedAtUtc = workout.CompletedAtUtc!.Value,
            })
            .ToListAsync(cancellationToken);
    }
}
