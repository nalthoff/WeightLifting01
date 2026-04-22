using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Commands.RemoveWorkoutLift;

public sealed class RemoveWorkoutLiftCommandHandler(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<RemoveWorkoutLiftResult> HandleAsync(
        RemoveWorkoutLiftCommand command,
        CancellationToken cancellationToken)
    {
        var workoutEntity = await dbContext.Workouts
            .SingleOrDefaultAsync(
                workout => workout.Id == command.WorkoutId && workout.UserId == DefaultUserId,
                cancellationToken);

        if (workoutEntity is null)
        {
            return new RemoveWorkoutLiftResult
            {
                Outcome = RemoveWorkoutLiftOutcome.NotFound,
            };
        }

        if (workoutEntity.Status != WorkoutStatus.InProgress)
        {
            return new RemoveWorkoutLiftResult
            {
                Outcome = RemoveWorkoutLiftOutcome.Conflict,
            };
        }

        var workoutLiftEntryEntity = await dbContext.WorkoutLiftEntries
            .SingleOrDefaultAsync(
                workoutLiftEntry => workoutLiftEntry.WorkoutId == command.WorkoutId
                    && workoutLiftEntry.Id == command.WorkoutLiftEntryId,
                cancellationToken);

        if (workoutLiftEntryEntity is null)
        {
            return new RemoveWorkoutLiftResult
            {
                Outcome = RemoveWorkoutLiftOutcome.NotFound,
            };
        }

        dbContext.WorkoutLiftEntries.Remove(workoutLiftEntryEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RemoveWorkoutLiftResult
        {
            Outcome = RemoveWorkoutLiftOutcome.Removed,
            WorkoutId = command.WorkoutId,
            WorkoutLiftEntryId = command.WorkoutLiftEntryId,
        };
    }
}
