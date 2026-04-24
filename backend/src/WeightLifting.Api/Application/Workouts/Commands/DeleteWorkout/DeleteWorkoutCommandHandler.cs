using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Commands.DeleteWorkout;

public sealed class DeleteWorkoutCommandHandler(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<DeleteWorkoutResult> HandleAsync(
        DeleteWorkoutCommand command,
        CancellationToken cancellationToken)
    {
        var workoutEntity = await dbContext.Workouts
            .SingleOrDefaultAsync(
                workout => workout.Id == command.WorkoutId && workout.UserId == DefaultUserId,
                cancellationToken);

        if (workoutEntity is null)
        {
            return new DeleteWorkoutResult
            {
                Outcome = DeleteWorkoutOutcome.NotFound,
            };
        }

        if (workoutEntity.Status != WorkoutStatus.InProgress)
        {
            return new DeleteWorkoutResult
            {
                Outcome = DeleteWorkoutOutcome.Conflict,
            };
        }

        var workoutSets = await dbContext.WorkoutSets
            .Where(set => set.WorkoutId == command.WorkoutId)
            .ToListAsync(cancellationToken);
        var workoutLiftEntries = await dbContext.WorkoutLiftEntries
            .Where(entry => entry.WorkoutId == command.WorkoutId)
            .ToListAsync(cancellationToken);

        dbContext.WorkoutSets.RemoveRange(workoutSets);
        dbContext.WorkoutLiftEntries.RemoveRange(workoutLiftEntries);
        dbContext.Workouts.Remove(workoutEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteWorkoutResult
        {
            Outcome = DeleteWorkoutOutcome.Deleted,
            WorkoutId = command.WorkoutId,
        };
    }
}
