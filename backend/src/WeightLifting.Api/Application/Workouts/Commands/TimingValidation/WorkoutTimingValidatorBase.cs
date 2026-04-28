using System.Globalization;

namespace WeightLifting.Api.Application.Workouts.Commands.TimingValidation;

public abstract class WorkoutTimingValidatorBase(TimeProvider timeProvider)
{
    public Dictionary<string, string[]> Validate(
        DateOnly trainingDayLocalDate,
        string? startTimeLocal,
        int sessionLengthMinutes)
    {
        var errors = new Dictionary<string, string[]>();

        ValidateTrainingDay(trainingDayLocalDate, errors);
        ValidateStartTimeLocal(startTimeLocal, errors);
        ValidateSessionLengthMinutes(sessionLengthMinutes, errors);

        return errors;
    }

    protected virtual void ValidateTrainingDay(
        DateOnly trainingDayLocalDate,
        Dictionary<string, string[]> errors)
    {
        _ = trainingDayLocalDate;
        _ = errors;
    }

    protected DateOnly GetCurrentUtcDate() => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

    private static void ValidateStartTimeLocal(
        string? startTimeLocal,
        Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(startTimeLocal))
        {
            errors["startTimeLocal"] = ["Start time is required in HH:mm format."];
            return;
        }

        var parsed = TimeOnly.TryParseExact(
            startTimeLocal,
            "HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
        if (!parsed)
        {
            errors["startTimeLocal"] = ["Start time must use HH:mm format."];
        }
    }

    private static void ValidateSessionLengthMinutes(
        int sessionLengthMinutes,
        Dictionary<string, string[]> errors)
    {
        if (sessionLengthMinutes <= 0)
        {
            errors["sessionLengthMinutes"] = ["Session length minutes must be greater than zero."];
        }
    }
}
