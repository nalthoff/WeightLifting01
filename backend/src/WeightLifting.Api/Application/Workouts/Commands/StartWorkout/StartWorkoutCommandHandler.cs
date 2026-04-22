using WeightLifting.Api.Application.Workouts.Queries.GetInProgressWorkout;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.StartWorkout;

public sealed class StartWorkoutCommandHandler(
    WeightLiftingDbContext dbContext,
    GetInProgressWorkoutQueryHelper getInProgressWorkoutQueryHelper)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<StartWorkoutResult> HandleAsync(
        StartWorkoutCommand command,
        CancellationToken cancellationToken)
    {
        var existingWorkout = await getInProgressWorkoutQueryHelper.GetAsync(DefaultUserId, cancellationToken);
        if (existingWorkout is not null)
        {
            return new StartWorkoutResult
            {
                Outcome = StartWorkoutOutcome.AlreadyInProgress,
                Workout = existingWorkout,
            };
        }

        var nowUtc = DateTime.UtcNow;
        var workout = new Workout(
            Guid.NewGuid(),
            DefaultUserId,
            WorkoutStatus.InProgress,
            command.Label,
            nowUtc,
            null,
            nowUtc,
            nowUtc);

        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workout.Id,
            UserId = workout.UserId,
            Status = workout.Status,
            Label = workout.Label,
            StartedAtUtc = workout.StartedAtUtc,
            CompletedAtUtc = workout.CompletedAtUtc,
            CreatedAtUtc = workout.CreatedAtUtc,
            UpdatedAtUtc = workout.UpdatedAtUtc,
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new StartWorkoutResult
        {
            Outcome = StartWorkoutOutcome.Created,
            Workout = workout,
        };
    }
}
