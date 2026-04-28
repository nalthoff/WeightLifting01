using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts;
using WeightLifting.Api.Application.Workouts.Commands.WorkoutEntryMutability;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.AddWorkoutLift;

public sealed class AddWorkoutLiftCommandHandler(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";
    private static readonly IWorkoutEntryMutabilityRule LiveMutabilityRule = new LiveWorkoutEntryMutabilityRule();
    private static readonly IWorkoutEntryMutabilityRule HistoricalMutabilityRule = new HistoricalWorkoutEntryMutabilityRule();

    public async Task<AddWorkoutLiftResult> HandleAsync(
        AddWorkoutLiftCommand command,
        CancellationToken cancellationToken)
    {
        var mutabilityCheck = await WorkoutEntryMutabilityService.CheckAsync(
            dbContext,
            command.WorkoutId,
            DefaultUserId,
            command.AllowHistoricalEdits ? HistoricalMutabilityRule : LiveMutabilityRule,
            cancellationToken);
        if (!mutabilityCheck.WorkoutExists)
        {
            throw new KeyNotFoundException("Workout was not found.");
        }

        if (!mutabilityCheck.CanMutate)
        {
            throw new WorkoutNotInProgressException(command.WorkoutId);
        }

        var liftEntity = await dbContext.Lifts
            .SingleOrDefaultAsync(lift => lift.Id == command.LiftId, cancellationToken);

        if (liftEntity is null)
        {
            throw new KeyNotFoundException("Lift was not found.");
        }

        if (!liftEntity.IsActive)
        {
            throw new LiftNotActiveException(command.LiftId);
        }

        var nextPosition = await dbContext.WorkoutLiftEntries
            .Where(workoutLiftEntry => workoutLiftEntry.WorkoutId == command.WorkoutId)
            .MaxAsync(workoutLiftEntry => (int?)workoutLiftEntry.Position, cancellationToken) ?? 0;

        var nowUtc = DateTime.UtcNow;
        var workoutLiftEntryEntity = new WorkoutLiftEntryEntity
        {
            Id = Guid.NewGuid(),
            WorkoutId = command.WorkoutId,
            LiftId = command.LiftId,
            DisplayName = liftEntity.Name,
            AddedAtUtc = nowUtc,
            Position = nextPosition + 1,
        };

        dbContext.WorkoutLiftEntries.Add(workoutLiftEntryEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AddWorkoutLiftResult
        {
            WorkoutLift = ToWorkoutLiftEntry(workoutLiftEntryEntity),
        };
    }

    private static WorkoutLiftEntry ToWorkoutLiftEntry(WorkoutLiftEntryEntity workoutLiftEntryEntity) => new()
    {
        Id = workoutLiftEntryEntity.Id,
        WorkoutId = workoutLiftEntryEntity.WorkoutId,
        LiftId = workoutLiftEntryEntity.LiftId,
        DisplayName = workoutLiftEntryEntity.DisplayName,
        AddedAtUtc = workoutLiftEntryEntity.AddedAtUtc,
        Position = workoutLiftEntryEntity.Position,
    };
}
