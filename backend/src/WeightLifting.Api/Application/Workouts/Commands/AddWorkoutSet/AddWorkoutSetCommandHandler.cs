using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Entities;

namespace WeightLifting.Api.Application.Workouts.Commands.AddWorkoutSet;

public sealed class AddWorkoutSetCommandHandler(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<AddWorkoutSetResult> HandleAsync(
        AddWorkoutSetCommand command,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(command);
        if (validationErrors.Count > 0)
        {
            return new AddWorkoutSetResult
            {
                Outcome = AddWorkoutSetOutcome.ValidationFailed,
                Errors = validationErrors,
            };
        }

        var workoutEntity = await dbContext.Workouts
            .SingleOrDefaultAsync(
                workout => workout.Id == command.WorkoutId && workout.UserId == DefaultUserId,
                cancellationToken);

        if (workoutEntity is null)
        {
            return new AddWorkoutSetResult
            {
                Outcome = AddWorkoutSetOutcome.NotFound,
            };
        }

        if (workoutEntity.Status != WorkoutStatus.InProgress)
        {
            return new AddWorkoutSetResult
            {
                Outcome = AddWorkoutSetOutcome.Conflict,
                Errors = new Dictionary<string, string[]>
                {
                    ["workout"] = ["Workout must be in progress to add sets."],
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
            return new AddWorkoutSetResult
            {
                Outcome = AddWorkoutSetOutcome.NotFound,
            };
        }

        var nextSetNumber = await dbContext.WorkoutSets
            .Where(workoutSet => workoutSet.WorkoutLiftEntryId == command.WorkoutLiftEntryId)
            .MaxAsync(workoutSet => (int?)workoutSet.SetNumber, cancellationToken) ?? 0;

        var nowUtc = DateTime.UtcNow;
        var workoutSetEntity = new WorkoutSetEntity
        {
            Id = Guid.NewGuid(),
            WorkoutId = command.WorkoutId,
            WorkoutLiftEntryId = command.WorkoutLiftEntryId,
            SetNumber = nextSetNumber + 1,
            Reps = command.Reps,
            Weight = command.Weight,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc,
        };

        dbContext.WorkoutSets.Add(workoutSetEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AddWorkoutSetResult
        {
            Outcome = AddWorkoutSetOutcome.Created,
            Set = ToWorkoutSetEntry(workoutSetEntity),
        };
    }

    private static Dictionary<string, string[]> Validate(AddWorkoutSetCommand command)
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

    private static WorkoutSetEntry ToWorkoutSetEntry(WorkoutSetEntity workoutSetEntity) => new()
    {
        Id = workoutSetEntity.Id,
        WorkoutId = workoutSetEntity.WorkoutId,
        WorkoutLiftEntryId = workoutSetEntity.WorkoutLiftEntryId,
        SetNumber = workoutSetEntity.SetNumber,
        Reps = workoutSetEntity.Reps,
        Weight = workoutSetEntity.Weight,
        CreatedAtUtc = workoutSetEntity.CreatedAtUtc,
    };
}
