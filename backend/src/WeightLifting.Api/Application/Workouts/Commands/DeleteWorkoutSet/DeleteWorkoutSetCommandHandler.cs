using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Commands.DeleteWorkoutSet;

public sealed class DeleteWorkoutSetCommandHandler(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<DeleteWorkoutSetResult> HandleAsync(
        DeleteWorkoutSetCommand command,
        CancellationToken cancellationToken)
    {
        var workoutEntity = await dbContext.Workouts
            .SingleOrDefaultAsync(
                workout => workout.Id == command.WorkoutId && workout.UserId == DefaultUserId,
                cancellationToken);

        if (workoutEntity is null)
        {
            return new DeleteWorkoutSetResult
            {
                Outcome = DeleteWorkoutSetOutcome.NotFound,
            };
        }

        if (workoutEntity.Status != WorkoutStatus.InProgress)
        {
            return new DeleteWorkoutSetResult
            {
                Outcome = DeleteWorkoutSetOutcome.Conflict,
            };
        }

        var workoutLiftEntryExists = await dbContext.WorkoutLiftEntries
            .AnyAsync(workoutLiftEntry =>
                    workoutLiftEntry.WorkoutId == command.WorkoutId
                    && workoutLiftEntry.Id == command.WorkoutLiftEntryId,
                cancellationToken);

        if (!workoutLiftEntryExists)
        {
            return new DeleteWorkoutSetResult
            {
                Outcome = DeleteWorkoutSetOutcome.NotFound,
            };
        }

        var workoutSetEntity = await dbContext.WorkoutSets
            .SingleOrDefaultAsync(
                workoutSet =>
                    workoutSet.Id == command.SetId
                    && workoutSet.WorkoutId == command.WorkoutId
                    && workoutSet.WorkoutLiftEntryId == command.WorkoutLiftEntryId,
                cancellationToken);

        if (workoutSetEntity is null)
        {
            return new DeleteWorkoutSetResult
            {
                Outcome = DeleteWorkoutSetOutcome.NotFound,
            };
        }

        dbContext.WorkoutSets.Remove(workoutSetEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteWorkoutSetResult
        {
            Outcome = DeleteWorkoutSetOutcome.Deleted,
            WorkoutId = command.WorkoutId,
            WorkoutLiftEntryId = command.WorkoutLiftEntryId,
            SetId = command.SetId,
        };
    }
}
