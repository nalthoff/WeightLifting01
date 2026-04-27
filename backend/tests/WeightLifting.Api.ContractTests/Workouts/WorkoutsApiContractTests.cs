using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WeightLifting.Api.ContractTests;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

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
    public async Task GetWorkoutForHistoryWhenWorkoutInProgressReturnsNotFoundPayload()
    {
        var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "In Progress History Probe",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdPayload = await createResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(createdPayload);

        var response = await client.GetAsync($"/api/workouts/{createdPayload.Workout.Id}?forHistory=true");

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
        Assert.Equal("Workout cannot be completed", payload.Title);
        Assert.Equal((int)HttpStatusCode.Conflict, payload.Status);
        Assert.Contains("Workout must be in progress to complete.", payload.Errors["workout"]);
    }

    [Fact]
    public async Task DeleteWorkoutReturnsSuccessContract()
    {
        var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "Delete Me",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdPayload = await createResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(createdPayload);

        var deleteResponse = await client.DeleteAsync($"/api/workouts/{createdPayload.Workout.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var payload = await deleteResponse.Content.ReadFromJsonAsync<DeleteWorkoutResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal(createdPayload.Workout.Id, payload.WorkoutId);
    }

    [Fact]
    public async Task DeleteWorkoutWhenMissingReturnsNotFoundPayload()
    {
        var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/workouts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<NotFoundResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Workout not found", payload.Title);
        Assert.Equal((int)HttpStatusCode.NotFound, payload.Status);
    }

    [Fact]
    public async Task DeleteWorkoutWhenAlreadyCompletedReturnsSuccessContract()
    {
        var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "Already Completed",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdPayload = await createResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(createdPayload);

        var completeResponse = await client.PostAsync($"/api/workouts/{createdPayload.Workout.Id}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/workouts/{createdPayload.Workout.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var payload = await deleteResponse.Content.ReadFromJsonAsync<DeleteWorkoutResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal(createdPayload.Workout.Id, payload.WorkoutId);
    }

    [Fact]
    public async Task GetWorkoutHistoryReturnsCompletedWorkoutsInNewestFirstOrder()
    {
        var client = factory.CreateClient();
        var firstCreate = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "First complete",
        });
        Assert.Equal(HttpStatusCode.Created, firstCreate.StatusCode);
        var firstPayload = await firstCreate.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(firstPayload);

        var firstComplete = await client.PostAsync($"/api/workouts/{firstPayload.Workout.Id}/complete", null);
        Assert.Equal(HttpStatusCode.OK, firstComplete.StatusCode);

        var secondCreate = await client.PostAsJsonAsync("/api/workouts", new
        {
            label = "",
        });
        Assert.Equal(HttpStatusCode.Created, secondCreate.StatusCode);
        var secondPayload = await secondCreate.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(secondPayload);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
            var now = DateTime.UtcNow;
            var squatLiftId = Guid.NewGuid();
            var benchLiftId = Guid.NewGuid();
            dbContext.Lifts.AddRange(
                new LiftEntity
                {
                    Id = squatLiftId,
                    Name = "Squat",
                    NameNormalized = "squat",
                    IsActive = true,
                    CreatedAtUtc = now,
                },
                new LiftEntity
                {
                    Id = benchLiftId,
                    Name = "Bench Press",
                    NameNormalized = "bench press",
                    IsActive = true,
                    CreatedAtUtc = now,
                });
            dbContext.WorkoutLiftEntries.AddRange(
                new WorkoutLiftEntryEntity
                {
                    Id = Guid.NewGuid(),
                    WorkoutId = secondPayload.Workout.Id,
                    LiftId = squatLiftId,
                    DisplayName = "Squat",
                    AddedAtUtc = now,
                    Position = 0,
                },
                new WorkoutLiftEntryEntity
                {
                    Id = Guid.NewGuid(),
                    WorkoutId = secondPayload.Workout.Id,
                    LiftId = benchLiftId,
                    DisplayName = "Bench",
                    AddedAtUtc = now,
                    Position = 1,
                });
            await dbContext.SaveChangesAsync();
        }

        var secondComplete = await client.PostAsync($"/api/workouts/{secondPayload.Workout.Id}/complete", null);
        Assert.Equal(HttpStatusCode.OK, secondComplete.StatusCode);

        var response = await client.GetAsync("/api/workouts/history");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<WorkoutHistoryResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal(2, payload.Items.Count);
        Assert.Equal(secondPayload.Workout.Id, payload.Items[0].WorkoutId);
        Assert.Equal("Workout", payload.Items[0].Label);
        Assert.Matches("^[0-9]{2}:[0-9]{2}$", payload.Items[0].DurationDisplay);
        Assert.Equal(2, payload.Items[0].LiftCount);
        Assert.Equal(firstPayload.Workout.Id, payload.Items[1].WorkoutId);
        Assert.Equal("First complete", payload.Items[1].Label);
        Assert.Matches("^[0-9]{2}:[0-9]{2}$", payload.Items[1].DurationDisplay);
        Assert.Equal(0, payload.Items[1].LiftCount);
        Assert.True(payload.Items[0].CompletedAtUtc >= payload.Items[1].CompletedAtUtc);
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

    public sealed class DeleteWorkoutResponse
    {
        public required Guid WorkoutId { get; init; }
    }

    public sealed class ValidationErrorResponse
    {
        public required string Title { get; init; }

        public required int Status { get; init; }

        public required Dictionary<string, string[]> Errors { get; init; }
    }

    public sealed class NotFoundResponse
    {
        public required string Title { get; init; }

        public required int Status { get; init; }
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
}
