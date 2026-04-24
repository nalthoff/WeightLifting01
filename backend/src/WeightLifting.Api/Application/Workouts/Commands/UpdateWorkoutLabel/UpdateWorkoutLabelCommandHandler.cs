using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutLabel;

public sealed class UpdateWorkoutLabelCommandHandler(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<UpdateWorkoutLabelResult> HandleAsync(
        UpdateWorkoutLabelCommand command,
        CancellationToken cancellationToken)
    {
        if (command.WorkoutId == Guid.Empty)
        {
            return new UpdateWorkoutLabelResult
            {
                Outcome = UpdateWorkoutLabelOutcome.ValidationFailed,
                Errors = new Dictionary<string, string[]>
                {
                    ["workoutId"] = ["Workout id is required."],
                },
            };
        }

        var workoutEntity = await dbContext.Workouts
            .SingleOrDefaultAsync(
                workout => workout.Id == command.WorkoutId && workout.UserId == DefaultUserId,
                cancellationToken);

        if (workoutEntity is null)
        {
            return new UpdateWorkoutLabelResult
            {
                Outcome = UpdateWorkoutLabelOutcome.NotFound,
            };
        }

        if (workoutEntity.Status != WorkoutStatus.InProgress)
        {
            return new UpdateWorkoutLabelResult
            {
                Outcome = UpdateWorkoutLabelOutcome.Conflict,
                Errors = new Dictionary<string, string[]>
                {
                    ["workout"] = ["Workout must be in progress to edit name."],
                },
            };
        }

        string? normalizedLabel;
        try
        {
            normalizedLabel = Workout.NormalizeLabel(command.Label);
        }
        catch (ArgumentException)
        {
            return new UpdateWorkoutLabelResult
            {
                Outcome = UpdateWorkoutLabelOutcome.ValidationFailed,
                Errors = new Dictionary<string, string[]>
                {
                    ["label"] = [$"Workout label must be {Workout.MaxLabelLength} characters or fewer."],
                },
            };
        }

        workoutEntity.Label = normalizedLabel;
        workoutEntity.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var workout = new Workout(
            workoutEntity.Id,
            workoutEntity.UserId,
            workoutEntity.Status,
            workoutEntity.Label,
            workoutEntity.StartedAtUtc,
            workoutEntity.CompletedAtUtc,
            workoutEntity.CreatedAtUtc,
            workoutEntity.UpdatedAtUtc);

        return new UpdateWorkoutLabelResult
        {
            Outcome = UpdateWorkoutLabelOutcome.Updated,
            Workout = workout,
        };
    }
}
