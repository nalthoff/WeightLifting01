using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Queries.ListWorkoutLifts;

public sealed class ListWorkoutLiftsQueryHelper(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<IReadOnlyList<WorkoutLiftEntry>> GetAsync(
        Guid workoutId,
        CancellationToken cancellationToken,
        bool requireCompleted = false)
    {
        var workout = await dbContext.Workouts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                workout => workout.Id == workoutId && workout.UserId == DefaultUserId,
                cancellationToken);

        if (workout is null)
        {
            throw new KeyNotFoundException("Workout was not found.");
        }

        if (requireCompleted && workout.Status != WorkoutStatus.Completed)
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
                Sets = dbContext.WorkoutSets
                    .Where(workoutSet => workoutSet.WorkoutLiftEntryId == workoutLiftEntry.Id)
                    .OrderBy(workoutSet => workoutSet.SetNumber)
                    .Select(workoutSet => new WorkoutSetEntry
                    {
                        Id = workoutSet.Id,
                        WorkoutId = workoutSet.WorkoutId,
                        WorkoutLiftEntryId = workoutSet.WorkoutLiftEntryId,
                        SetNumber = workoutSet.SetNumber,
                        Reps = workoutSet.Reps,
                        Weight = workoutSet.Weight,
                        CreatedAtUtc = workoutSet.CreatedAtUtc,
                        UpdatedAtUtc = workoutSet.UpdatedAtUtc,
                    })
                    .ToList(),
            })
            .ToListAsync(cancellationToken);
    }
}
