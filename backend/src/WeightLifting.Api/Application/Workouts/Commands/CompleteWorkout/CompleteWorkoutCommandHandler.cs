using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Commands.CompleteWorkout;

public sealed class CompleteWorkoutCommandHandler(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<CompleteWorkoutResult> HandleAsync(
        CompleteWorkoutCommand command,
        CancellationToken cancellationToken)
    {
        var workoutEntity = await dbContext.Workouts
            .SingleOrDefaultAsync(
                workout => workout.Id == command.WorkoutId && workout.UserId == DefaultUserId,
                cancellationToken);

        if (workoutEntity is null)
        {
            return new CompleteWorkoutResult
            {
                Outcome = CompleteWorkoutOutcome.NotFound,
            };
        }

        if (workoutEntity.Status != WorkoutStatus.InProgress)
        {
            return new CompleteWorkoutResult
            {
                Outcome = CompleteWorkoutOutcome.Conflict,
            };
        }

        var completedWorkout = new Workout(
            workoutEntity.Id,
            workoutEntity.UserId,
            workoutEntity.Status,
            workoutEntity.Label,
            workoutEntity.StartedAtUtc,
            workoutEntity.CompletedAtUtc,
            workoutEntity.CreatedAtUtc,
            workoutEntity.UpdatedAtUtc)
            .Complete(DateTime.UtcNow);

        workoutEntity.Status = completedWorkout.Status;
        workoutEntity.CompletedAtUtc = completedWorkout.CompletedAtUtc;
        workoutEntity.UpdatedAtUtc = completedWorkout.UpdatedAtUtc;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CompleteWorkoutResult
        {
            Outcome = CompleteWorkoutOutcome.Completed,
            Workout = completedWorkout,
        };
    }
}
