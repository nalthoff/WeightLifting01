using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WeightLifting.Api.ContractTests;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.ContractTests.Workouts;

public sealed class WorkoutsApiContractTests(LiftsContractWebApplicationFactory factory)
    : IClassFixture<LiftsContractWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task PostWorkoutReturnsCreatedWorkoutContract()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "  Contract Session  ",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.NotEqual(Guid.Empty, payload.Workout.Id);
        Assert.Equal("InProgress", payload.Workout.Status);
        Assert.Equal("Contract Session", payload.Workout.Label);
    }

    [Fact]
    public async Task GetWorkoutReturnsWorkoutContractById()
    {
        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "Deep Link Session",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdPayload = await createResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(createdPayload);

        var getResponse = await client.GetAsync($"/api/workouts/{createdPayload.Workout.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getPayload = await getResponse.Content.ReadFromJsonAsync<GetWorkoutResponse>(JsonOptions);
        Assert.NotNull(getPayload);
        Assert.Equal(createdPayload.Workout.Id, getPayload.Workout.Id);
        Assert.Equal("InProgress", getPayload.Workout.Status);
        Assert.Equal("Deep Link Session", getPayload.Workout.Label);
    }

    [Fact]
    public async Task GetWorkoutWhenMissingReturnsNotFoundPayload()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/workouts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<NotFoundResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Workout not found", payload.Title);
        Assert.Equal((int)HttpStatusCode.NotFound, payload.Status);
    }

    [Fact]
    public async Task GetActiveWorkoutWhenNoWorkoutExistsReturnsNoContent()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/workouts/active");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetActiveWorkoutReturnsSummaryContract()
    {
        var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "Home Session",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var response = await client.GetAsync("/api/workouts/active");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ActiveWorkoutSummaryResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("InProgress", payload.Workout.Status);
        Assert.Equal("Home Session", payload.Workout.Label);
        Assert.Null(payload.Workout.CompletedAtUtc);
    }

    [Fact]
    public async Task PostWorkoutWhenAlreadyInProgressReturnsConflictPayload()
    {
        var client = factory.CreateClient();

        var firstResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "First Session",
        });
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var conflictResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "Second Session",
        });

        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);

        var payload = await conflictResponse.Content.ReadFromJsonAsync<ExistingInProgressWorkoutResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal("Workout already in progress", payload.Title);
        Assert.Equal((int)HttpStatusCode.Conflict, payload.Status);
        Assert.NotEqual(Guid.Empty, payload.Workout.Id);
        Assert.Equal("InProgress", payload.Workout.Status);
        Assert.Equal("First Session", payload.Workout.Label);
    }

    [Fact]
    public async Task PostWorkoutWithLabelTooLongReturnsValidationPayloadAndDoesNotCreateWorkout()
    {
        var client = factory.CreateClient();
        var tooLongLabel = new string('x', Workout.MaxLabelLength + 1);

        var invalidResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = tooLongLabel,
        });

        Assert.Equal((HttpStatusCode)422, invalidResponse.StatusCode);

        var validationPayload = await invalidResponse.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);

        Assert.NotNull(validationPayload);
        Assert.Contains(
            $"Workout label must be {Workout.MaxLabelLength} characters or fewer.",
            validationPayload.Errors["label"]);

        // If the invalid request created a hidden in-progress workout, this follow-up would return 409.
        var validResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "Recovery Session",
        });

        Assert.Equal(HttpStatusCode.Created, validResponse.StatusCode);
    }

    [Fact]
    public async Task PostCompleteWorkoutReturnsCompletedWorkoutContract()
    {
        var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "Completion Session",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdPayload = await createResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(createdPayload);

        var completeResponse = await client.PostAsync($"/api/workouts/{createdPayload.Workout.Id}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var payload = await completeResponse.Content.ReadFromJsonAsync<CompleteWorkoutResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal(createdPayload.Workout.Id, payload.Workout.Id);
        Assert.Equal("Completed", payload.Workout.Status);
        Assert.NotNull(payload.Workout.CompletedAtUtc);
    }

    [Fact]
    public async Task PostCompleteWorkoutWhenMissingReturnsNotFoundPayload()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync($"/api/workouts/{Guid.NewGuid()}/complete", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<NotFoundResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Workout not found", payload.Title);
        Assert.Equal((int)HttpStatusCode.NotFound, payload.Status);
    }

    [Fact]
    public async Task PostCompleteWorkoutWhenAlreadyCompletedReturnsConflictPayload()
    {
        var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "Already Done",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdPayload = await createResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(createdPayload);

        var firstComplete = await client.PostAsync($"/api/workouts/{createdPayload.Workout.Id}/complete", null);
        Assert.Equal(HttpStatusCode.OK, firstComplete.StatusCode);

        var secondComplete = await client.PostAsync($"/api/workouts/{createdPayload.Workout.Id}/complete", null);
        Assert.Equal(HttpStatusCode.Conflict, secondComplete.StatusCode);

        var payload = await secondComplete.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Contains("Workout must be in progress to complete.", payload.Errors["workout"]);
    }

    public async Task InitializeAsync()
    {
        using var client = factory.CreateClient();
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

    public sealed class ExistingInProgressWorkoutResponse
    {
        public required string Title { get; init; }

        public required int Status { get; init; }

        public required WorkoutSessionSummaryResponse Workout { get; init; }
    }

    public sealed class GetWorkoutResponse
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

    public sealed class ActiveWorkoutSummaryResponse
    {
        public required WorkoutSessionSummaryResponse Workout { get; init; }
    }

    public sealed class CompleteWorkoutResponse
    {
        public required WorkoutSessionSummaryResponse Workout { get; init; }
    }

    public sealed class ValidationErrorResponse
    {
        public required Dictionary<string, string[]> Errors { get; init; }
    }

    public sealed class NotFoundResponse
    {
        public required string Title { get; init; }

        public required int Status { get; init; }
    }
}
