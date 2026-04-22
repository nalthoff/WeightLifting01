using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Queries.ListWorkoutLifts;

public sealed class ListWorkoutLiftsQueryHelper(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<IReadOnlyList<WorkoutLiftEntry>> GetAsync(
        Guid workoutId,
        CancellationToken cancellationToken)
    {
        var hasWorkout = await dbContext.Workouts
            .AsNoTracking()
            .AnyAsync(
                workout => workout.Id == workoutId && workout.UserId == DefaultUserId,
                cancellationToken);

        if (!hasWorkout)
        {
            throw new KeyNotFoundException("Workout was not found.");
        }

        return await dbContext.WorkoutLiftEntries
            .AsNoTracking()
            .Where(workoutLiftEntry => workoutLiftEntry.WorkoutId == workoutId)
            .OrderBy(workoutLiftEntry => workoutLiftEntry.Position)
            .Select(workoutLiftEntry => new WorkoutLiftEntry
            {
                Id = workoutLiftEntry.Id,
                WorkoutId = workoutLiftEntry.WorkoutId,
                LiftId = workoutLiftEntry.LiftId,
                DisplayName = workoutLiftEntry.DisplayName,
                AddedAtUtc = workoutLiftEntry.AddedAtUtc,
                Position = workoutLiftEntry.Position,
            })
            .ToListAsync(cancellationToken);
    }
}
