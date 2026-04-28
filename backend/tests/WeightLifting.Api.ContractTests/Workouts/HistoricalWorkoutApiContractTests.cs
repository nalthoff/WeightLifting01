using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.ContractTests.Workouts;

public sealed class HistoricalWorkoutApiContractTests(LiftsContractWebApplicationFactory factory)
    : IClassFixture<LiftsContractWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task PostHistoricalWorkoutReturnsCreatedWorkoutContract()
    {
        var client = factory.CreateClient();
        var trainingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));

        var response = await client.PostAsJsonAsync("/api/workouts/historical", new
        {
            trainingDayLocalDate = trainingDay,
            startTimeLocal = "07:30",
            sessionLengthMinutes = 90,
            label = "Historical Contract Session",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.NotEqual(Guid.Empty, payload.Workout.Id);
        Assert.Equal("Completed", payload.Workout.Status);
        Assert.Equal("Historical Contract Session", payload.Workout.Label);
        Assert.NotNull(payload.Workout.CompletedAtUtc);
        Assert.Equal(TimeSpan.FromMinutes(90), payload.Workout.CompletedAtUtc!.Value - payload.Workout.StartedAtUtc);
    }

    [Fact]
    public async Task PostHistoricalWorkoutWhenRequiredJsonMembersAreMissingReturnsBadRequestValidationPayload()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/workouts/historical", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<BadRequestValidationResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal((int)HttpStatusCode.BadRequest, payload.Status);
        Assert.Contains("request", payload.Errors.Keys);
        Assert.Contains("$", payload.Errors.Keys);

        var allErrors = string.Join(" ", payload.Errors.SelectMany(static item => item.Value));
        Assert.Contains("trainingDayLocalDate", allErrors, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("startTimeLocal", allErrors, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sessionLengthMinutes", allErrors, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PostHistoricalWorkoutWhenRequiredValuesAreInvalidReturnsValidationPayload()
    {
        var client = factory.CreateClient();
        var trainingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var response = await client.PostAsJsonAsync("/api/workouts/historical", new
        {
            trainingDayLocalDate = trainingDay,
            startTimeLocal = "",
            sessionLengthMinutes = 0,
        });

        Assert.Equal((HttpStatusCode)422, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Validation failed", payload.Title);
        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, payload.Status);
        Assert.Contains("Start time is required in HH:mm format.", payload.Errors["startTimeLocal"]);
        Assert.Contains("Session length minutes must be greater than zero.", payload.Errors["sessionLengthMinutes"]);
    }

    public async Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public sealed class StartWorkoutCreatedResponse
    {
        public required WorkoutSessionSummaryResponse Workout { get; init; }
    }

    public sealed class WorkoutSessionSummaryResponse
    {
        public required Guid Id { get; init; }

        public required string Status { get; init; }

        public string? Label { get; init; }

        public required DateTime StartedAtUtc { get; init; }

        public DateTime? CompletedAtUtc { get; init; }
    }

    public sealed class ValidationErrorResponse
    {
        public required string Title { get; init; }

        public required int Status { get; init; }

        public required Dictionary<string, string[]> Errors { get; init; }
    }

    public sealed class BadRequestValidationResponse
    {
        public required int Status { get; init; }

        public required Dictionary<string, string[]> Errors { get; init; }
    }
}
