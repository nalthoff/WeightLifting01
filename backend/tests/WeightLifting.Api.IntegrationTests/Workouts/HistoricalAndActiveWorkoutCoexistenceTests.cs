using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class HistoricalAndActiveWorkoutCoexistenceTests(HistoricalWorkoutLifecycleTests.HistoricalWorkoutWebApplicationFactory factory)
    : IClassFixture<HistoricalWorkoutLifecycleTests.HistoricalWorkoutWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task HistoricalWorkoutLifecycleDoesNotDisruptActiveWorkoutAccessOrStatus()
    {
        using var client = factory.CreateClient();
        var activeCreateResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "Current Active Session",
        });
        Assert.Equal(HttpStatusCode.Created, activeCreateResponse.StatusCode);
        var activeCreated = await activeCreateResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(activeCreated);
        Assert.Equal("InProgress", activeCreated.Workout.Status);

        var historicalCreateResponse = await client.PostAsJsonAsync("/api/workouts/historical", new
        {
            trainingDayLocalDate = new DateOnly(2026, 4, 23),
            startTimeLocal = "06:10",
            sessionLengthMinutes = 50,
            label = "Backfilled Pull Day",
        });
        Assert.Equal(HttpStatusCode.Created, historicalCreateResponse.StatusCode);
        var historicalCreated = await historicalCreateResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(historicalCreated);
        Assert.Equal("Completed", historicalCreated.Workout.Status);
        Assert.NotNull(historicalCreated.Workout.CompletedAtUtc);

        var activeSummaryResponse = await client.GetAsync("/api/workouts/active");
        Assert.Equal(HttpStatusCode.OK, activeSummaryResponse.StatusCode);
        var activeSummary = await activeSummaryResponse.Content.ReadFromJsonAsync<ActiveWorkoutSummaryResponse>(JsonOptions);
        Assert.NotNull(activeSummary);
        Assert.Equal(activeCreated.Workout.Id, activeSummary.Workout.Id);
        Assert.Equal("InProgress", activeSummary.Workout.Status);
        Assert.Equal("Current Active Session", activeSummary.Workout.Label);
        Assert.Null(activeSummary.Workout.CompletedAtUtc);

        var activeDetailsResponse = await client.GetAsync($"/api/workouts/{activeCreated.Workout.Id}");
        Assert.Equal(HttpStatusCode.OK, activeDetailsResponse.StatusCode);
        var activeDetails = await activeDetailsResponse.Content.ReadFromJsonAsync<GetWorkoutResponse>(JsonOptions);
        Assert.NotNull(activeDetails);
        Assert.Equal(activeCreated.Workout.Id, activeDetails.Workout.Id);
        Assert.Equal("InProgress", activeDetails.Workout.Status);

        var historyResponse = await client.GetAsync("/api/workouts/history");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);
        var historyPayload = await historyResponse.Content.ReadFromJsonAsync<WorkoutHistoryResponse>(JsonOptions);
        Assert.NotNull(historyPayload);
        Assert.Contains(historyPayload.Items, item => item.WorkoutId == historicalCreated.Workout.Id);
        Assert.DoesNotContain(historyPayload.Items, item => item.WorkoutId == activeCreated.Workout.Id);
    }

    [Fact]
    public async Task CompletingAlreadyCompletedHistoricalWorkoutReturnsExplicitConflictAndPreservesActiveWorkout()
    {
        using var client = factory.CreateClient();
        var activeCreateResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "Current Active Session",
        });
        Assert.Equal(HttpStatusCode.Created, activeCreateResponse.StatusCode);
        var activeCreated = await activeCreateResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(activeCreated);

        var historicalCreateResponse = await client.PostAsJsonAsync("/api/workouts/historical", new
        {
            trainingDayLocalDate = new DateOnly(2026, 4, 22),
            startTimeLocal = "06:10",
            sessionLengthMinutes = 50,
            label = "Backfilled Pull Day",
        });
        Assert.Equal(HttpStatusCode.Created, historicalCreateResponse.StatusCode);
        var historicalCreated = await historicalCreateResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(historicalCreated);

        var completeHistoricalResponse = await client.PostAsync($"/api/workouts/{historicalCreated.Workout.Id}/complete", null);
        Assert.Equal(HttpStatusCode.Conflict, completeHistoricalResponse.StatusCode);
        var conflictPayload = await completeHistoricalResponse.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(conflictPayload);
        Assert.Equal("Workout cannot be completed", conflictPayload.Title);
        Assert.Equal((int)HttpStatusCode.Conflict, conflictPayload.Status);
        Assert.Contains("Workout must be in progress to complete.", conflictPayload.Errors["workout"]);

        var activeSummaryResponse = await client.GetAsync("/api/workouts/active");
        Assert.Equal(HttpStatusCode.OK, activeSummaryResponse.StatusCode);
        var activeSummary = await activeSummaryResponse.Content.ReadFromJsonAsync<ActiveWorkoutSummaryResponse>(JsonOptions);
        Assert.NotNull(activeSummary);
        Assert.Equal(activeCreated.Workout.Id, activeSummary.Workout.Id);
        Assert.Equal("InProgress", activeSummary.Workout.Status);
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

    public sealed class ActiveWorkoutSummaryResponse
    {
        public required WorkoutSessionSummaryResponse Workout { get; init; }
    }

    public sealed class GetWorkoutResponse
    {
        public required WorkoutSessionSummaryResponse Workout { get; init; }

        public required IReadOnlyList<object> Lifts { get; init; }
    }

    public sealed class WorkoutSessionSummaryResponse
    {
        public required Guid Id { get; init; }

        public required string Status { get; init; }

        public string? Label { get; init; }

        public required DateTime StartedAtUtc { get; init; }

        public DateTime? CompletedAtUtc { get; init; }
    }

    public sealed class WorkoutHistoryResponse
    {
        public required IReadOnlyList<WorkoutHistoryItemResponse> Items { get; init; }
    }

    public sealed class WorkoutHistoryItemResponse
    {
        public required Guid WorkoutId { get; init; }

        public required string Label { get; init; }

        public required DateTime CompletedAtUtc { get; init; }

        public required string DurationDisplay { get; init; }

        public required int LiftCount { get; init; }
    }

    public sealed class ValidationErrorResponse
    {
        public required string Title { get; init; }

        public required int Status { get; init; }

        public required Dictionary<string, string[]> Errors { get; init; }
    }
}
