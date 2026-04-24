using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WeightLifting.Api.ContractTests;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.ContractTests.Workouts;

public sealed class WorkoutLabelApiContractTests(LiftsContractWebApplicationFactory factory)
    : IClassFixture<LiftsContractWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task PutWorkoutLabelReturnsUpdatedWorkoutContract()
    {
        var client = factory.CreateClient();
        var workoutId = await CreateWorkoutAsync(client, "Before");

        var response = await client.PutAsJsonAsync($"/api/workouts/{workoutId}/label", new
        {
            label = "  Updated  ",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<UpdateWorkoutLabelResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal(workoutId, payload.Workout.Id);
        Assert.Equal("InProgress", payload.Workout.Status);
        Assert.Equal("Updated", payload.Workout.Label);
    }

    [Fact]
    public async Task PutWorkoutLabelWithWhitespaceClearsLabel()
    {
        var client = factory.CreateClient();
        var workoutId = await CreateWorkoutAsync(client, "Named");

        var response = await client.PutAsJsonAsync($"/api/workouts/{workoutId}/label", new
        {
            label = "   ",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<UpdateWorkoutLabelResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Null(payload.Workout.Label);
    }

    [Fact]
    public async Task PutWorkoutLabelForCompletedWorkoutReturnsConflictContract()
    {
        var client = factory.CreateClient();
        var workoutId = await CreateWorkoutAsync(client, "Completed");
        var completeResponse = await client.PostAsync($"/api/workouts/{workoutId}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var response = await client.PutAsJsonAsync($"/api/workouts/{workoutId}/label", new
        {
            label = "Should Fail",
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Contains("Workout must be in progress to edit name.", payload.Errors["workout"]);
    }

    [Fact]
    public async Task PutWorkoutLabelWithTooLongLabelReturnsValidationContract()
    {
        var client = factory.CreateClient();
        var workoutId = await CreateWorkoutAsync(client, "Before");

        var response = await client.PutAsJsonAsync($"/api/workouts/{workoutId}/label", new
        {
            label = new string('x', Workout.MaxLabelLength + 1),
        });

        Assert.Equal((HttpStatusCode)422, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Contains($"Workout label must be {Workout.MaxLabelLength} characters or fewer.", payload.Errors["label"]);
    }

    public async Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task<Guid> CreateWorkoutAsync(HttpClient client, string label)
    {
        var response = await client.PostAsJsonAsync("/api/workouts", new { label });
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(payload);
        return payload.Workout.Id;
    }

    public sealed class StartWorkoutCreatedResponse
    {
        public required WorkoutSessionSummaryResponse Workout { get; init; }
    }

    public sealed class UpdateWorkoutLabelResponse
    {
        public required WorkoutSessionSummaryResponse Workout { get; init; }
    }

    public sealed class WorkoutSessionSummaryResponse
    {
        public required Guid Id { get; init; }

        public required string Status { get; init; }

        public string? Label { get; init; }
    }

    public sealed class ValidationErrorResponse
    {
        public required Dictionary<string, string[]> Errors { get; init; }
    }
}
