using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeightLifting.Api.ContractTests;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.ContractTests.Workouts;

public sealed class WorkoutLiftsApiContractTests(LiftsContractWebApplicationFactory factory)
    : IClassFixture<LiftsContractWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task PostWorkoutLiftReturnsCreatedContract()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Contract Workout");
        var lift = await CreateLiftAsync(client, "Contract Front Squat");

        var response = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new
        {
            liftId = lift.Id,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.NotEqual(Guid.Empty, payload.WorkoutLift.Id);
        Assert.Equal(workout.Id, payload.WorkoutLift.WorkoutId);
        Assert.Equal(lift.Id, payload.WorkoutLift.LiftId);
        Assert.Equal("Contract Front Squat", payload.WorkoutLift.DisplayName);
        Assert.Equal(1, payload.WorkoutLift.Position);
    }

    [Fact]
    public async Task GetWorkoutLiftsReturnsListContract()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "List Workout");
        var firstLift = await CreateLiftAsync(client, "List Bench Press");
        var secondLift = await CreateLiftAsync(client, "List Deadlift");

        await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = firstLift.Id });
        await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = secondLift.Id });

        var response = await client.GetAsync($"/api/workouts/{workout.Id}/lifts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<WorkoutLiftListResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal(2, payload.Items.Count);
        Assert.Equal(1, payload.Items[0].Position);
        Assert.Equal(2, payload.Items[1].Position);
        Assert.Equal(firstLift.Id, payload.Items[0].LiftId);
        Assert.Equal(secondLift.Id, payload.Items[1].LiftId);
    }

    [Fact]
    public async Task PostWorkoutLiftAllowsDuplicateLiftEntries()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Duplicate Workout");
        var lift = await CreateLiftAsync(client, "Duplicate Bench Press");

        var firstResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new
        {
            liftId = lift.Id,
        });
        var secondResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new
        {
            liftId = lift.Id,
        });

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/workouts/{workout.Id}/lifts");
        var listPayload = await listResponse.Content.ReadFromJsonAsync<WorkoutLiftListResponse>(JsonOptions);

        Assert.NotNull(listPayload);
        Assert.Equal(2, listPayload.Items.Count);
        Assert.All(listPayload.Items, item => Assert.Equal(lift.Id, item.LiftId));
    }

    [Fact]
    public async Task DeleteWorkoutLiftReturnsSuccessContract()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Remove Workout");
        var lift = await CreateLiftAsync(client, "Remove Front Squat");
        var addResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new
        {
            liftId = lift.Id,
        });
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        var addedPayload = await addResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(addedPayload);

        var response = await client.DeleteAsync($"/api/workouts/{workout.Id}/lifts/{addedPayload.WorkoutLift.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<RemoveWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal(workout.Id, payload.WorkoutId);
        Assert.Equal(addedPayload.WorkoutLift.Id, payload.WorkoutLiftEntryId);

        var listResponse = await client.GetAsync($"/api/workouts/{workout.Id}/lifts");
        var listPayload = await listResponse.Content.ReadFromJsonAsync<WorkoutLiftListResponse>(JsonOptions);
        Assert.NotNull(listPayload);
        Assert.Empty(listPayload.Items);
    }

    [Fact]
    public async Task DeleteWorkoutLiftWithMissingWorkoutOrEntryReturnsNotFoundPayload()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Delete Not Found Workout");
        var lift = await CreateLiftAsync(client, "Delete Not Found Lift");
        var addResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new
        {
            liftId = lift.Id,
        });
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        var addedPayload = await addResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(addedPayload);

        var missingWorkoutResponse = await client.DeleteAsync($"/api/workouts/{Guid.NewGuid()}/lifts/{addedPayload.WorkoutLift.Id}");
        var missingEntryResponse = await client.DeleteAsync($"/api/workouts/{workout.Id}/lifts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, missingWorkoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingEntryResponse.StatusCode);

        var missingWorkoutPayload = await missingWorkoutResponse.Content.ReadFromJsonAsync<NotFoundResponse>(JsonOptions);
        var missingEntryPayload = await missingEntryResponse.Content.ReadFromJsonAsync<NotFoundResponse>(JsonOptions);

        Assert.NotNull(missingWorkoutPayload);
        Assert.NotNull(missingEntryPayload);
        Assert.Equal("Resource not found", missingWorkoutPayload.Title);
        Assert.Equal("Resource not found", missingEntryPayload.Title);
        Assert.Equal((int)HttpStatusCode.NotFound, missingWorkoutPayload.Status);
        Assert.Equal((int)HttpStatusCode.NotFound, missingEntryPayload.Status);
    }

    [Fact]
    public async Task DeleteWorkoutLiftWithCompletedWorkoutReturnsConflictPayload()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Delete Conflict Workout");
        var lift = await CreateLiftAsync(client, "Delete Conflict Lift");
        var addResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new
        {
            liftId = lift.Id,
        });
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        var addedPayload = await addResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(addedPayload);
        await MarkWorkoutCompletedAsync(workout.Id);

        var response = await client.DeleteAsync($"/api/workouts/{workout.Id}/lifts/{addedPayload.WorkoutLift.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Contains("Workout must be in progress to remove lifts.", payload.Errors["workout"]);
    }

    [Fact]
    public async Task PutReorderWorkoutLiftsReturnsSuccessContract()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Reorder Contract Workout");
        var firstLift = await CreateLiftAsync(client, "Reorder Contract Front Squat");
        var secondLift = await CreateLiftAsync(client, "Reorder Contract Deadlift");

        var firstAddResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new
        {
            liftId = firstLift.Id,
        });
        var secondAddResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new
        {
            liftId = secondLift.Id,
        });

        var firstAdded = await firstAddResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        var secondAdded = await secondAddResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(firstAdded);
        Assert.NotNull(secondAdded);

        var response = await client.PutAsJsonAsync($"/api/workouts/{workout.Id}/lifts/reorder", new
        {
            orderedWorkoutLiftEntryIds = new[] { secondAdded.WorkoutLift.Id, firstAdded.WorkoutLift.Id },
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ReorderWorkoutLiftsResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal(workout.Id, payload.WorkoutId);
        Assert.Equal(2, payload.Items.Count);
        Assert.Equal(secondAdded.WorkoutLift.Id, payload.Items[0].Id);
        Assert.Equal(firstAdded.WorkoutLift.Id, payload.Items[1].Id);
        Assert.Equal(1, payload.Items[0].Position);
        Assert.Equal(2, payload.Items[1].Position);
    }

    [Fact]
    public async Task PutReorderWorkoutLiftsReturnsNotFoundForMissingWorkoutOrEntry()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Reorder Not Found Workout");
        var firstLift = await CreateLiftAsync(client, "Reorder Not Found Lift 1");
        var secondLift = await CreateLiftAsync(client, "Reorder Not Found Lift 2");

        var firstAddResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = firstLift.Id });
        var secondAddResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = secondLift.Id });
        var firstAdded = await firstAddResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        var secondAdded = await secondAddResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(firstAdded);
        Assert.NotNull(secondAdded);

        var missingWorkoutResponse = await client.PutAsJsonAsync($"/api/workouts/{Guid.NewGuid()}/lifts/reorder", new
        {
            orderedWorkoutLiftEntryIds = new[] { firstAdded.WorkoutLift.Id, secondAdded.WorkoutLift.Id },
        });
        var missingEntryResponse = await client.PutAsJsonAsync($"/api/workouts/{workout.Id}/lifts/reorder", new
        {
            orderedWorkoutLiftEntryIds = new[] { firstAdded.WorkoutLift.Id, Guid.NewGuid() },
        });

        Assert.Equal(HttpStatusCode.NotFound, missingWorkoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingEntryResponse.StatusCode);
    }

    [Fact]
    public async Task PutReorderWorkoutLiftsReturnsConflictAndValidationContracts()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Reorder Failure Workout");
        var firstLift = await CreateLiftAsync(client, "Reorder Failure Lift 1");
        var secondLift = await CreateLiftAsync(client, "Reorder Failure Lift 2");

        var firstAddResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = firstLift.Id });
        var secondAddResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = secondLift.Id });
        var firstAdded = await firstAddResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        var secondAdded = await secondAddResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(firstAdded);
        Assert.NotNull(secondAdded);

        var validationResponse = await client.PutAsJsonAsync($"/api/workouts/{workout.Id}/lifts/reorder", new
        {
            orderedWorkoutLiftEntryIds = new[] { firstAdded.WorkoutLift.Id, firstAdded.WorkoutLift.Id },
        });

        Assert.Equal((HttpStatusCode)422, validationResponse.StatusCode);
        var validationPayload = await validationResponse.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(validationPayload);
        Assert.Contains("A complete ordered set of workout lift entry ids is required.", validationPayload.Errors["orderedWorkoutLiftEntryIds"]);

        await MarkWorkoutCompletedAsync(workout.Id);

        var conflictResponse = await client.PutAsJsonAsync($"/api/workouts/{workout.Id}/lifts/reorder", new
        {
            orderedWorkoutLiftEntryIds = new[] { secondAdded.WorkoutLift.Id, firstAdded.WorkoutLift.Id },
        });

        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
        var conflictPayload = await conflictResponse.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(conflictPayload);
        Assert.Contains("Workout must be in progress to reorder lifts.", conflictPayload.Errors["workout"]);
    }

    [Fact]
    public async Task PostWorkoutLiftWithMissingLiftIdReturnsValidationPayload()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Validation Workout");

        var response = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new
        {
            liftId = Guid.Empty,
        });

        Assert.Equal((HttpStatusCode)422, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Contains("Lift id is required.", payload.Errors["liftId"]);
    }

    [Fact]
    public async Task PostWorkoutLiftWithMissingWorkoutOrLiftReturnsNotFoundPayload()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Not Found Workout");
        var lift = await CreateLiftAsync(client, "Not Found Lift");

        var missingWorkoutResponse = await client.PostAsJsonAsync($"/api/workouts/{Guid.NewGuid()}/lifts", new
        {
            liftId = lift.Id,
        });

        var missingLiftResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new
        {
            liftId = Guid.NewGuid(),
        });

        Assert.Equal(HttpStatusCode.NotFound, missingWorkoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingLiftResponse.StatusCode);

        var missingWorkoutPayload = await missingWorkoutResponse.Content.ReadFromJsonAsync<NotFoundErrorResponse>(JsonOptions);
        var missingLiftPayload = await missingLiftResponse.Content.ReadFromJsonAsync<NotFoundErrorResponse>(JsonOptions);

        Assert.NotNull(missingWorkoutPayload);
        Assert.NotNull(missingLiftPayload);
        Assert.Equal("Resource not found", missingWorkoutPayload.Title);
        Assert.Equal("Workout was not found.", missingWorkoutPayload.Detail);
        Assert.Equal("Lift was not found.", missingLiftPayload.Detail);
    }

    [Fact]
    public async Task PostWorkoutLiftWithCompletedWorkoutReturnsConflictPayload()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Completed Workout");
        var lift = await CreateLiftAsync(client, "Conflict Lift");
        await MarkWorkoutCompletedAsync(workout.Id);

        var response = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new
        {
            liftId = lift.Id,
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Contains("Workout must be in progress to add lifts.", payload.Errors["workout"]);
    }

    [Fact]
    public async Task PostWorkoutLiftWithInactiveLiftReturnsValidationPayload()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Inactive Lift Workout");
        var inactiveLift = await CreateLiftAsync(client, "Inactive Lift");
        await MarkLiftInactiveAsync(inactiveLift.Id);

        var response = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new
        {
            liftId = inactiveLift.Id,
        });

        Assert.Equal((HttpStatusCode)422, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Contains("Lift must be active to add to the workout.", payload.Errors["liftId"]);
    }

    public async Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task<WorkoutSummaryResponse> CreateWorkoutAsync(HttpClient client, string label)
    {
        var response = await client.PostAsJsonAsync("/api/workouts", new { label });
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(payload);
        return payload.Workout;
    }

    private static async Task<LiftSummaryResponse> CreateLiftAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/lifts", new { name });
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);
        Assert.NotNull(payload);
        return payload.Lift;
    }

    private async Task MarkWorkoutCompletedAsync(Guid workoutId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
        var workout = await dbContext.Workouts.SingleAsync(entity => entity.Id == workoutId);
        workout.Status = WorkoutStatus.Completed;
        workout.UpdatedAtUtc = new DateTime(2026, 4, 22, 13, 0, 0, DateTimeKind.Utc);
        await dbContext.SaveChangesAsync();
    }

    private async Task MarkLiftInactiveAsync(Guid liftId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
        var lift = await dbContext.Lifts.SingleAsync(entity => entity.Id == liftId);
        lift.IsActive = false;
        await dbContext.SaveChangesAsync();
    }

    public sealed class StartWorkoutCreatedResponse
    {
        public required WorkoutSummaryResponse Workout { get; init; }
    }

    public sealed class WorkoutSummaryResponse
    {
        public required Guid Id { get; init; }
    }

    public sealed class CreateLiftResponse
    {
        public required LiftSummaryResponse Lift { get; init; }
    }

    public sealed class LiftSummaryResponse
    {
        public required Guid Id { get; init; }
    }

    public sealed class AddWorkoutLiftResponse
    {
        public required WorkoutLiftEntryResponse WorkoutLift { get; init; }
    }

    public sealed class WorkoutLiftListResponse
    {
        public required IReadOnlyList<WorkoutLiftEntryResponse> Items { get; init; }
    }

    public sealed class RemoveWorkoutLiftResponse
    {
        public required Guid WorkoutId { get; init; }

        public required Guid WorkoutLiftEntryId { get; init; }
    }

    public sealed class ReorderWorkoutLiftsResponse
    {
        public required Guid WorkoutId { get; init; }

        public required IReadOnlyList<WorkoutLiftEntryResponse> Items { get; init; }
    }

    public sealed class WorkoutLiftEntryResponse
    {
        public required Guid Id { get; init; }

        public required Guid WorkoutId { get; init; }

        public required Guid LiftId { get; init; }

        public required string DisplayName { get; init; }

        public required DateTime AddedAtUtc { get; init; }

        public required int Position { get; init; }
    }

    public sealed class ValidationErrorResponse
    {
        public required Dictionary<string, string[]> Errors { get; init; }
    }

    public sealed class NotFoundErrorResponse
    {
        public required string Title { get; init; }

        public required int Status { get; init; }

        public required string Detail { get; init; }
    }

    public sealed class NotFoundResponse
    {
        public required string Title { get; init; }

        public required int Status { get; init; }
    }
}
