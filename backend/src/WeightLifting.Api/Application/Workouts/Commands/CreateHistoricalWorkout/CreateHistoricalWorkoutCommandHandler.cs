using WeightLifting.Api.Application.Workouts.Commands.TimingValidation;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.Application.Workouts.Commands.CreateHistoricalWorkout;

public sealed class CreateHistoricalWorkoutCommandHandler(
    WeightLiftingDbContext dbContext,
    IHistoricalWorkoutTimingValidator historicalWorkoutTimingValidator)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    public async Task<CreateHistoricalWorkoutResult> HandleAsync(
        CreateHistoricalWorkoutCommand command,
        CancellationToken cancellationToken)
    {
        var errors = historicalWorkoutTimingValidator.Validate(
            command.TrainingDayLocalDate,
            command.StartTimeLocal,
            command.SessionLengthMinutes);
        if (errors.Count > 0)
        {
            return new CreateHistoricalWorkoutResult
            {
                Outcome = CreateHistoricalWorkoutOutcome.ValidationFailed,
                Errors = errors,
            };
        }

        var startTimeLocal = TimeOnly.ParseExact(command.StartTimeLocal, "HH:mm");

        var startedAtUtc = DateTime.SpecifyKind(
            command.TrainingDayLocalDate.ToDateTime(startTimeLocal),
            DateTimeKind.Utc);
        var completedAtUtc = startedAtUtc.AddMinutes(command.SessionLengthMinutes);

        try
        {
            var workout = new Workout(
                Guid.NewGuid(),
                DefaultUserId,
                WorkoutStatus.Completed,
                command.Label,
                startedAtUtc,
                completedAtUtc,
                startedAtUtc,
                completedAtUtc);

            dbContext.Workouts.Add(ToWorkoutEntity(workout));

            await dbContext.SaveChangesAsync(cancellationToken);

            return new CreateHistoricalWorkoutResult
            {
                Outcome = CreateHistoricalWorkoutOutcome.Created,
                Workout = workout,
            };
        }
        catch (ArgumentException)
        {
            if (command.Label is not null)
            {
                errors["label"] = [$"Workout label must be {Workout.MaxLabelLength} characters or fewer."];
            }

            return new CreateHistoricalWorkoutResult
            {
                Outcome = CreateHistoricalWorkoutOutcome.ValidationFailed,
                Errors = errors,
            };
        }
    }

    private static WorkoutEntity ToWorkoutEntity(Workout workout) => new()
    {
        Id = workout.Id,
        UserId = workout.UserId,
        Status = workout.Status,
        Label = workout.Label,
        StartedAtUtc = workout.StartedAtUtc,
        CompletedAtUtc = workout.CompletedAtUtc,
        CreatedAtUtc = workout.CreatedAtUtc,
        UpdatedAtUtc = workout.UpdatedAtUtc,
    };
}
