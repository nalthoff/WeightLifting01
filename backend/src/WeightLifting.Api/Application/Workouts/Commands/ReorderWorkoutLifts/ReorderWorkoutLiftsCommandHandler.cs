using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Commands.ReorderWorkoutLifts;

public sealed class ReorderWorkoutLiftsCommandHandler(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<ReorderWorkoutLiftsResult> HandleAsync(
        ReorderWorkoutLiftsCommand command,
        CancellationToken cancellationToken)
    {
        var workoutEntity = await dbContext.Workouts
            .SingleOrDefaultAsync(
                workout => workout.Id == command.WorkoutId && workout.UserId == DefaultUserId,
                cancellationToken);

        if (workoutEntity is null)
        {
            return new ReorderWorkoutLiftsResult
            {
                Outcome = ReorderWorkoutLiftsOutcome.NotFound,
            };
        }

        if (workoutEntity.Status != WorkoutStatus.InProgress)
        {
            return new ReorderWorkoutLiftsResult
            {
                Outcome = ReorderWorkoutLiftsOutcome.Conflict,
            };
        }

        var orderedEntryIds = command.OrderedWorkoutLiftEntryIds;
        if (orderedEntryIds.Count == 0 || orderedEntryIds.Any(entryId => entryId == Guid.Empty))
        {
            return new ReorderWorkoutLiftsResult
            {
                Outcome = ReorderWorkoutLiftsOutcome.ValidationFailed,
            };
        }

        var workoutLiftEntryEntities = await dbContext.WorkoutLiftEntries
            .Where(workoutLiftEntry => workoutLiftEntry.WorkoutId == command.WorkoutId)
            .ToListAsync(cancellationToken);

        if (workoutLiftEntryEntities.Count == 0)
        {
            return new ReorderWorkoutLiftsResult
            {
                Outcome = ReorderWorkoutLiftsOutcome.ValidationFailed,
            };
        }

        var uniqueOrderedEntryIds = orderedEntryIds.Distinct().Count();
        if (uniqueOrderedEntryIds != orderedEntryIds.Count || orderedEntryIds.Count != workoutLiftEntryEntities.Count)
        {
            return new ReorderWorkoutLiftsResult
            {
                Outcome = ReorderWorkoutLiftsOutcome.ValidationFailed,
            };
        }

        var entitiesById = workoutLiftEntryEntities.ToDictionary(workoutLiftEntry => workoutLiftEntry.Id);
        if (orderedEntryIds.Any(entryId => !entitiesById.ContainsKey(entryId)))
        {
            return new ReorderWorkoutLiftsResult
            {
                Outcome = ReorderWorkoutLiftsOutcome.NotFound,
            };
        }

        // Update via a temporary non-overlapping range first to avoid unique-index collisions.
        for (var index = 0; index < orderedEntryIds.Count; index++)
        {
            var entryId = orderedEntryIds[index];
            entitiesById[entryId].Position = -(index + 1);
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        for (var index = 0; index < orderedEntryIds.Count; index++)
        {
            var entryId = orderedEntryIds[index];
            entitiesById[entryId].Position = index + 1;
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        var setsByEntryId = await dbContext.WorkoutSets
            .AsNoTracking()
            .Where(workoutSet => workoutSet.WorkoutId == command.WorkoutId)
            .GroupBy(workoutSet => workoutSet.WorkoutLiftEntryId)
            .ToDictionaryAsync(
                group => group.Key,
                group => (IReadOnlyList<WorkoutSetEntry>)group
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
                    })
                    .ToList(),
                cancellationToken);

        return new ReorderWorkoutLiftsResult
        {
            Outcome = ReorderWorkoutLiftsOutcome.Reordered,
            WorkoutId = command.WorkoutId,
            Items = orderedEntryIds
                .Select(entryId => ToWorkoutLiftEntry(
                    entitiesById[entryId],
                    setsByEntryId.GetValueOrDefault(entryId, [])))
                .ToList(),
        };
    }

    private static WorkoutLiftEntry ToWorkoutLiftEntry(
        Infrastructure.Persistence.Workouts.WorkoutLiftEntryEntity workoutLiftEntryEntity,
        IReadOnlyList<WorkoutSetEntry> sets) => new()
    {
        Id = workoutLiftEntryEntity.Id,
        WorkoutId = workoutLiftEntryEntity.WorkoutId,
        LiftId = workoutLiftEntryEntity.LiftId,
        DisplayName = workoutLiftEntryEntity.DisplayName,
        AddedAtUtc = workoutLiftEntryEntity.AddedAtUtc,
        Position = workoutLiftEntryEntity.Position,
        Sets = sets,
    };
}
