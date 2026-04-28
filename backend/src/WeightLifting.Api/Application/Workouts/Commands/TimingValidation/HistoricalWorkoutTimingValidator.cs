namespace WeightLifting.Api.Application.Workouts.Commands.TimingValidation;

public sealed class HistoricalWorkoutTimingValidator(TimeProvider timeProvider)
    : WorkoutTimingValidatorBase(timeProvider), IHistoricalWorkoutTimingValidator
{
    protected override void ValidateTrainingDay(
        DateOnly trainingDayLocalDate,
        Dictionary<string, string[]> errors)
    {
        var currentUtcDate = GetCurrentUtcDate();
        if (trainingDayLocalDate >= currentUtcDate)
        {
            errors["trainingDayLocalDate"] = ["Training day must be in the past."];
        }
    }
}
