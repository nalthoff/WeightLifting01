using System.Globalization;
using WeightLifting.Api.Application.Workouts.Commands.TimingValidation;

namespace WeightLifting.Api.UnitTests.Application.Workouts.CreateHistoricalWorkout;

public sealed class CreateHistoricalWorkoutCommandHandlerTests
{
    [Fact]
    public void ValidateWhenStartTimeMissingReturnsError()
    {
        var validator = CreateBaseValidator(new DateTimeOffset(2026, 4, 28, 12, 0, 0, TimeSpan.Zero));

        var errors = validator.Validate(new DateOnly(2026, 4, 20), null, 30);

        Assert.Equal(["Start time is required in HH:mm format."], errors["startTimeLocal"]);
    }

    [Fact]
    public void ValidateWhenStartTimeHasInvalidFormatReturnsError()
    {
        var validator = CreateBaseValidator(new DateTimeOffset(2026, 4, 28, 12, 0, 0, TimeSpan.Zero));

        var errors = validator.Validate(new DateOnly(2026, 4, 20), "9:30", 30);

        Assert.Equal(["Start time must use HH:mm format."], errors["startTimeLocal"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-15)]
    public void ValidateWhenDurationIsNotPositiveReturnsError(int sessionLengthMinutes)
    {
        var validator = CreateBaseValidator(new DateTimeOffset(2026, 4, 28, 12, 0, 0, TimeSpan.Zero));

        var errors = validator.Validate(new DateOnly(2026, 4, 20), "09:30", sessionLengthMinutes);

        Assert.Equal(["Session length minutes must be greater than zero."], errors["sessionLengthMinutes"]);
    }

    [Theory]
    [InlineData("2026-04-28")]
    [InlineData("2026-04-29")]
    public void ValidateHistoricalWhenTrainingDayIsNotPastReturnsError(string trainingDayIso)
    {
        var validator = CreateHistoricalValidator(new DateTimeOffset(2026, 4, 28, 12, 0, 0, TimeSpan.Zero));
        var trainingDay = DateOnly.Parse(trainingDayIso, CultureInfo.InvariantCulture);

        var errors = validator.Validate(trainingDay, "09:30", 30);

        Assert.Equal(["Training day must be in the past."], errors["trainingDayLocalDate"]);
    }

    [Fact]
    public void ValidateHistoricalWithValidInputsReturnsNoErrors()
    {
        var validator = CreateHistoricalValidator(new DateTimeOffset(2026, 4, 28, 12, 0, 0, TimeSpan.Zero));

        var errors = validator.Validate(new DateOnly(2026, 4, 27), "09:30", 45);

        Assert.Empty(errors);
    }

    private static TestWorkoutTimingValidator CreateBaseValidator(DateTimeOffset utcNow)
    {
        return new TestWorkoutTimingValidator(new FixedTimeProvider(utcNow));
    }

    private static HistoricalWorkoutTimingValidator CreateHistoricalValidator(DateTimeOffset utcNow)
    {
        return new HistoricalWorkoutTimingValidator(new FixedTimeProvider(utcNow));
    }

    private sealed class TestWorkoutTimingValidator(TimeProvider timeProvider)
        : WorkoutTimingValidatorBase(timeProvider);

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
