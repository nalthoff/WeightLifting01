namespace WeightLifting.Api.Application.Workouts.Commands.TimingValidation;

public interface IHistoricalWorkoutTimingValidator
{
    Dictionary<string, string[]> Validate(
        DateOnly trainingDayLocalDate,
        string? startTimeLocal,
        int sessionLengthMinutes);
}
