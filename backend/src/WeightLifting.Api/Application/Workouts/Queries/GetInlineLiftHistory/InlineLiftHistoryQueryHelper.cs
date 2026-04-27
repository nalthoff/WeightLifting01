using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Queries.GetInlineLiftHistory;

public sealed class InlineLiftHistoryQueryHelper(WeightLiftingDbContext dbContext)
{
    private const string DefaultUserId = "default-user";
    private const int MaxHistoryItems = 3;

    public async Task<IReadOnlyList<InlineLiftHistorySession>> GetAsync(
        Guid workoutId,
        Guid workoutLiftEntryId,
        CancellationToken cancellationToken)
    {
        var workout = await dbContext.Workouts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == workoutId && item.UserId == DefaultUserId,
                cancellationToken);

        if (workout is null)
        {
            throw new KeyNotFoundException("Workout was not found.");
        }

        if (workout.Status != WorkoutStatus.InProgress)
        {
            throw new InvalidOperationException("Workout must be in progress to load inline history.");
        }

        var workoutLiftEntry = await dbContext.WorkoutLiftEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == workoutLiftEntryId && item.WorkoutId == workoutId,
                cancellationToken);

        if (workoutLiftEntry is null)
        {
            throw new KeyNotFoundException("Workout lift entry was not found.");
        }

        var sessionRows = await (
                from workoutRow in dbContext.Workouts.AsNoTracking()
                join liftEntry in dbContext.WorkoutLiftEntries.AsNoTracking()
                    on workoutRow.Id equals liftEntry.WorkoutId
                where workoutRow.UserId == DefaultUserId
                      && workoutRow.Status == WorkoutStatus.Completed
                      && workoutRow.CompletedAtUtc.HasValue
                      && liftEntry.LiftId == workoutLiftEntry.LiftId
                      && dbContext.WorkoutSets.Any(setRow => setRow.WorkoutLiftEntryId == liftEntry.Id)
                orderby workoutRow.CompletedAtUtc descending
                select new
                {
                    WorkoutId = workoutRow.Id,
                    workoutRow.Label,
                    CompletedAtUtc = workoutRow.CompletedAtUtc!.Value,
                    WorkoutLiftEntryId = liftEntry.Id,
                })
            .Take(MaxHistoryItems)
            .ToListAsync(cancellationToken);

        if (sessionRows.Count == 0)
        {
            return [];
        }

        var sessionEntryIds = sessionRows.Select(item => item.WorkoutLiftEntryId).ToArray();
        var setRows = await dbContext.WorkoutSets
            .AsNoTracking()
            .Where(item => sessionEntryIds.Contains(item.WorkoutLiftEntryId))
            .OrderBy(item => item.SetNumber)
            .Select(item => new
            {
                item.WorkoutLiftEntryId,
                item.SetNumber,
                item.Reps,
                item.Weight,
            })
            .ToListAsync(cancellationToken);

        var setsByEntryId = setRows
            .GroupBy(item => item.WorkoutLiftEntryId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<InlineLiftHistorySet>)group
                    .Select(item => new InlineLiftHistorySet
                    {
                        SetNumber = item.SetNumber,
                        Reps = item.Reps,
                        Weight = item.Weight,
                    })
                    .ToList());

        return sessionRows
            .Select(item => new InlineLiftHistorySession
            {
                WorkoutId = item.WorkoutId,
                WorkoutLabel = item.Label,
                CompletedAtUtc = item.CompletedAtUtc,
                Sets = setsByEntryId.GetValueOrDefault(item.WorkoutLiftEntryId, []),
            })
            .ToList();
    }
}
