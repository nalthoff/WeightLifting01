using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutSet;

public sealed class UpdateWorkoutSetCommandHandler(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<UpdateWorkoutSetResult> HandleAsync(
        UpdateWorkoutSetCommand command,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(command);
        if (validationErrors.Count > 0)
        {
            return new UpdateWorkoutSetResult
            {
                Outcome = UpdateWorkoutSetOutcome.ValidationFailed,
                Errors = validationErrors,
            };
        }

        var workoutEntity = await dbContext.Workouts
            .SingleOrDefaultAsync(
                workout => workout.Id == command.WorkoutId && workout.UserId == DefaultUserId,
                cancellationToken);

        if (workoutEntity is null)
        {
            return new UpdateWorkoutSetResult
            {
                Outcome = UpdateWorkoutSetOutcome.NotFound,
            };
        }

        if (workoutEntity.Status != WorkoutStatus.InProgress)
        {
            return new UpdateWorkoutSetResult
            {
                Outcome = UpdateWorkoutSetOutcome.Conflict,
                Errors = new Dictionary<string, string[]>
                {
                    ["workout"] = ["Workout must be in progress to update sets."],
                },
            };
        }

        var workoutLiftEntryExists = await dbContext.WorkoutLiftEntries
            .AnyAsync(workoutLiftEntry =>
                    workoutLiftEntry.WorkoutId == command.WorkoutId
                    && workoutLiftEntry.Id == command.WorkoutLiftEntryId,
                cancellationToken);

        if (!workoutLiftEntryExists)
        {
            return new UpdateWorkoutSetResult
            {
                Outcome = UpdateWorkoutSetOutcome.NotFound,
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
            return new UpdateWorkoutSetResult
            {
                Outcome = UpdateWorkoutSetOutcome.NotFound,
            };
        }

        workoutSetEntity.Reps = command.Reps;
        workoutSetEntity.Weight = command.Weight;
        workoutSetEntity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateWorkoutSetResult
        {
            Outcome = UpdateWorkoutSetOutcome.Updated,
            Set = ToWorkoutSetEntry(workoutSetEntity),
        };
    }

    private static Dictionary<string, string[]> Validate(UpdateWorkoutSetCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        if (command.WorkoutId == Guid.Empty)
        {
            errors["workoutId"] = ["Workout id is required."];
        }

        if (command.WorkoutLiftEntryId == Guid.Empty)
        {
            errors["workoutLiftEntryId"] = ["Workout lift entry id is required."];
        }

        if (command.SetId == Guid.Empty)
        {
            errors["setId"] = ["Set id is required."];
        }

        if (command.Reps <= 0)
        {
            errors["reps"] = ["Reps must be greater than zero."];
        }

        if (command.Weight.HasValue && command.Weight.Value < 0)
        {
            errors["weight"] = ["Weight must be greater than or equal to zero when provided."];
        }

        return errors;
    }

    private static WorkoutSetEntry ToWorkoutSetEntry(Infrastructure.Persistence.Entities.WorkoutSetEntity workoutSetEntity) => new()
    {
        Id = workoutSetEntity.Id,
        WorkoutId = workoutSetEntity.WorkoutId,
        WorkoutLiftEntryId = workoutSetEntity.WorkoutLiftEntryId,
        SetNumber = workoutSetEntity.SetNumber,
        Reps = workoutSetEntity.Reps,
        Weight = workoutSetEntity.Weight,
        CreatedAtUtc = workoutSetEntity.CreatedAtUtc,
        UpdatedAtUtc = workoutSetEntity.UpdatedAtUtc,
    };
}
