using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeightLifting.Api.ContractTests;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Entities;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

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
    public async Task GetWorkoutLiftsForHistoryWhenWorkoutInProgressReturnsNotFound()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "In Progress Lift History Probe");
        var response = await client.GetAsync($"/api/workouts/{workout.Id}/lifts?forHistory=true");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<NotFoundResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Workout not found", payload.Title);
        Assert.Equal((int)HttpStatusCode.NotFound, payload.Status);
    }

    [Fact]
    public async Task GetInlineLiftHistoryReturnsExactLiftCompletedSessionsLimitedToThree()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Inline History Active Workout");
        var lift = await CreateLiftAsync(client, "Inline History Bench Press");
        var addLiftResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = lift.Id });
        var addLiftPayload = await addLiftResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(addLiftPayload);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
            var now = DateTime.UtcNow;

            for (var i = 1; i <= 4; i++)
            {
                var completedWorkoutId = Guid.NewGuid();
                var completedEntryId = Guid.NewGuid();
                dbContext.Workouts.Add(new WorkoutEntity
                {
                    Id = completedWorkoutId,
                    UserId = "default-user",
                    Status = WorkoutStatus.Completed,
                    Label = $"Completed {i}",
                    StartedAtUtc = now.AddDays(-i).AddHours(-1),
                    CompletedAtUtc = now.AddDays(-i),
                    CreatedAtUtc = now.AddDays(-i).AddHours(-1),
                    UpdatedAtUtc = now.AddDays(-i),
                });
                dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
                {
                    Id = completedEntryId,
                    WorkoutId = completedWorkoutId,
                    LiftId = lift.Id,
                    DisplayName = "Inline History Bench Press",
                    AddedAtUtc = now.AddDays(-i).AddMinutes(-30),
                    Position = 1,
                });
                dbContext.WorkoutSets.Add(new WorkoutSetEntity
                {
                    Id = Guid.NewGuid(),
                    WorkoutId = completedWorkoutId,
                    WorkoutLiftEntryId = completedEntryId,
                    SetNumber = 1,
                    Reps = 5 + i,
                    Weight = 200 + i,
                    CreatedAtUtc = now.AddDays(-i).AddMinutes(-20),
                    UpdatedAtUtc = now.AddDays(-i).AddMinutes(-20),
                });
            }

            await dbContext.SaveChangesAsync();
        }

        var response = await client.GetAsync($"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/history");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<InlineLiftHistoryResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal(workout.Id, payload.WorkoutId);
        Assert.Equal(addLiftPayload.WorkoutLift.Id, payload.WorkoutLiftEntryId);
        Assert.Equal(3, payload.Items.Count);
        Assert.All(payload.Items, item => Assert.Single(item.Sets));
        Assert.True(payload.Items[0].CompletedAtUtc >= payload.Items[1].CompletedAtUtc);
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
    public async Task PostWorkoutSetReturnsCreatedContract()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Set Contract Workout");
        var lift = await CreateLiftAsync(client, "Set Contract Front Squat");
        var addLiftResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = lift.Id });
        var addLiftPayload = await addLiftResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(addLiftPayload);

        var response = await client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets",
            new
            {
                reps = 5,
                weight = 225m,
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CreateWorkoutSetResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal(workout.Id, payload.WorkoutId);
        Assert.Equal(addLiftPayload.WorkoutLift.Id, payload.WorkoutLiftEntryId);
        Assert.NotEqual(Guid.Empty, payload.Set.Id);
        Assert.Equal(1, payload.Set.SetNumber);
        Assert.Equal(5, payload.Set.Reps);
        Assert.Equal(225m, payload.Set.Weight);
    }

    [Fact]
    public async Task PutWorkoutSetReturnsSuccessContract()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Set Update Contract Workout");
        var lift = await CreateLiftAsync(client, "Set Update Contract Front Squat");
        var addLiftResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = lift.Id });
        var addLiftPayload = await addLiftResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(addLiftPayload);

        var createSetResponse = await client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets",
            new { reps = 5, weight = 225m });
        var createSetPayload = await createSetResponse.Content.ReadFromJsonAsync<CreateWorkoutSetResponse>(JsonOptions);
        Assert.NotNull(createSetPayload);

        var response = await client.PutAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets/{createSetPayload.Set.Id}",
            new { reps = 7, weight = 230m });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<UpdateWorkoutSetResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal(workout.Id, payload.WorkoutId);
        Assert.Equal(addLiftPayload.WorkoutLift.Id, payload.WorkoutLiftEntryId);
        Assert.Equal(createSetPayload.Set.Id, payload.Set.Id);
        Assert.Equal(1, payload.Set.SetNumber);
        Assert.Equal(7, payload.Set.Reps);
        Assert.Equal(230m, payload.Set.Weight);
    }

    [Fact]
    public async Task PostWorkoutSetAllowsDuplicateLiftEntriesWithIndependentSetNumbering()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Set Duplicate Contract Workout");
        var lift = await CreateLiftAsync(client, "Set Duplicate Bench Press");
        var firstEntryResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = lift.Id });
        var secondEntryResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = lift.Id });
        var firstEntry = await firstEntryResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        var secondEntry = await secondEntryResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(firstEntry);
        Assert.NotNull(secondEntry);

        var firstSet = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts/{firstEntry.WorkoutLift.Id}/sets", new { reps = 8, weight = 155m });
        var secondSet = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts/{secondEntry.WorkoutLift.Id}/sets", new { reps = 10, weight = 135m });
        var secondSetPayload = await secondSet.Content.ReadFromJsonAsync<CreateWorkoutSetResponse>(JsonOptions);
        Assert.NotNull(secondSetPayload);

        Assert.Equal(HttpStatusCode.Created, firstSet.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondSet.StatusCode);
        Assert.Equal(1, secondSetPayload.Set.SetNumber);
    }

    [Fact]
    public async Task GetWorkoutLiftsReturnsPersistedSetsAfterAddSet()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Set List Contract Workout");
        var lift = await CreateLiftAsync(client, "Set List Back Squat");
        var addLiftResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = lift.Id });
        var addLiftPayload = await addLiftResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(addLiftPayload);

        var firstSetResponse = await client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets",
            new { reps = 5, weight = 225m });
        var secondSetResponse = await client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets",
            new { reps = 3, weight = (decimal?)null });

        Assert.Equal(HttpStatusCode.Created, firstSetResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondSetResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/workouts/{workout.Id}/lifts");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var listPayload = await listResponse.Content.ReadFromJsonAsync<WorkoutLiftListResponse>(JsonOptions);
        Assert.NotNull(listPayload);
        Assert.Single(listPayload.Items);

        var sets = listPayload.Items[0].Sets;
        Assert.NotNull(sets);
        Assert.Equal(2, sets.Count);
        Assert.Equal([1, 2], sets.Select(set => set.SetNumber).ToArray());
        Assert.Equal([5, 3], sets.Select(set => set.Reps).ToArray());
        Assert.Null(sets[1].Weight);
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
    public async Task PutReorderWorkoutLiftsPreservesSetsOnResponseItems()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Reorder Sets Contract Workout");
        var firstLift = await CreateLiftAsync(client, "Reorder Sets Bench");
        var secondLift = await CreateLiftAsync(client, "Reorder Sets Deadlift");

        var firstAddResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = firstLift.Id });
        var secondAddResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = secondLift.Id });
        var firstAdded = await firstAddResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        var secondAdded = await secondAddResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(firstAdded);
        Assert.NotNull(secondAdded);

        var addSetResponse = await client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{firstAdded.WorkoutLift.Id}/sets",
            new { reps = 5, weight = 225m });
        Assert.Equal(HttpStatusCode.Created, addSetResponse.StatusCode);

        var reorderResponse = await client.PutAsJsonAsync($"/api/workouts/{workout.Id}/lifts/reorder", new
        {
            orderedWorkoutLiftEntryIds = new[] { secondAdded.WorkoutLift.Id, firstAdded.WorkoutLift.Id },
        });

        Assert.Equal(HttpStatusCode.OK, reorderResponse.StatusCode);

        var payload = await reorderResponse.Content.ReadFromJsonAsync<ReorderWorkoutLiftsResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal(2, payload.Items.Count);
        Assert.Equal(secondAdded.WorkoutLift.Id, payload.Items[0].Id);
        Assert.Equal(firstAdded.WorkoutLift.Id, payload.Items[1].Id);
        Assert.Empty(payload.Items[0].Sets);
        Assert.Single(payload.Items[1].Sets);
        Assert.Equal(1, payload.Items[1].Sets[0].SetNumber);
        Assert.Equal(5, payload.Items[1].Sets[0].Reps);
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

    [Fact]
    public async Task PostWorkoutSetReturnsNotFoundConflictAndValidationContracts()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Set Failure Contract Workout");
        var lift = await CreateLiftAsync(client, "Set Failure Deadlift");
        var addLiftResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = lift.Id });
        var addLiftPayload = await addLiftResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(addLiftPayload);

        var missingWorkout = await client.PostAsJsonAsync(
            $"/api/workouts/{Guid.NewGuid()}/lifts/{addLiftPayload.WorkoutLift.Id}/sets",
            new { reps = 5, weight = 225m });

        var missingEntry = await client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{Guid.NewGuid()}/sets",
            new { reps = 5, weight = 225m });

        var invalidPayload = await client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets",
            new { reps = 0, weight = -1m });

        await MarkWorkoutCompletedAsync(workout.Id);
        var conflictPayload = await client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets",
            new { reps = 5, weight = 225m });

        Assert.Equal(HttpStatusCode.NotFound, missingWorkout.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingEntry.StatusCode);
        Assert.Equal((HttpStatusCode)422, invalidPayload.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, conflictPayload.StatusCode);

        var validation = await invalidPayload.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        var conflict = await conflictPayload.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(validation);
        Assert.NotNull(conflict);
        Assert.Contains("Reps must be greater than zero.", validation.Errors["reps"]);
        Assert.Contains("Weight must be greater than or equal to zero when provided.", validation.Errors["weight"]);
        Assert.Contains("Workout must be in progress to add sets.", conflict.Errors["workout"]);
    }

    [Fact]
    public async Task PutWorkoutSetReturnsNotFoundConflictAndValidationContracts()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Set Update Failure Contract Workout");
        var lift = await CreateLiftAsync(client, "Set Update Failure Deadlift");
        var addLiftResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = lift.Id });
        var addLiftPayload = await addLiftResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(addLiftPayload);

        var createSetResponse = await client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets",
            new { reps = 5, weight = 225m });
        var createSetPayload = await createSetResponse.Content.ReadFromJsonAsync<CreateWorkoutSetResponse>(JsonOptions);
        Assert.NotNull(createSetPayload);

        var missingWorkout = await client.PutAsJsonAsync(
            $"/api/workouts/{Guid.NewGuid()}/lifts/{addLiftPayload.WorkoutLift.Id}/sets/{createSetPayload.Set.Id}",
            new { reps = 6, weight = 220m });

        var missingEntry = await client.PutAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{Guid.NewGuid()}/sets/{createSetPayload.Set.Id}",
            new { reps = 6, weight = 220m });

        var invalidPayload = await client.PutAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets/{createSetPayload.Set.Id}",
            new { reps = 0, weight = -1m });

        await MarkWorkoutCompletedAsync(workout.Id);
        var conflictPayload = await client.PutAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets/{createSetPayload.Set.Id}",
            new { reps = 6, weight = 220m });

        Assert.Equal(HttpStatusCode.NotFound, missingWorkout.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingEntry.StatusCode);
        Assert.Equal((HttpStatusCode)422, invalidPayload.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, conflictPayload.StatusCode);

        var validation = await invalidPayload.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        var conflict = await conflictPayload.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(validation);
        Assert.NotNull(conflict);
        Assert.Contains("Reps must be greater than zero.", validation.Errors["reps"]);
        Assert.Contains("Weight must be greater than or equal to zero when provided.", validation.Errors["weight"]);
        Assert.Contains("Workout must be in progress to update sets.", conflict.Errors["workout"]);
    }

    [Fact]
    public async Task DeleteWorkoutSetReturnsSuccessContract()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Set Delete Contract Workout");
        var lift = await CreateLiftAsync(client, "Set Delete Contract Bench");
        var addLiftResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = lift.Id });
        var addLiftPayload = await addLiftResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(addLiftPayload);

        var createSetResponse = await client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets",
            new { reps = 5, weight = 225m });
        var createSetPayload = await createSetResponse.Content.ReadFromJsonAsync<CreateWorkoutSetResponse>(JsonOptions);
        Assert.NotNull(createSetPayload);

        var response = await client.DeleteAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets/{createSetPayload.Set.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<DeleteWorkoutSetResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal(workout.Id, payload.WorkoutId);
        Assert.Equal(addLiftPayload.WorkoutLift.Id, payload.WorkoutLiftEntryId);
        Assert.Equal(createSetPayload.Set.Id, payload.SetId);
    }

    [Fact]
    public async Task DeleteWorkoutSetReturnsNotFoundAndConflictContracts()
    {
        var client = factory.CreateClient();
        var workout = await CreateWorkoutAsync(client, "Set Delete Failure Contract Workout");
        var lift = await CreateLiftAsync(client, "Set Delete Failure Deadlift");
        var addLiftResponse = await client.PostAsJsonAsync($"/api/workouts/{workout.Id}/lifts", new { liftId = lift.Id });
        var addLiftPayload = await addLiftResponse.Content.ReadFromJsonAsync<AddWorkoutLiftResponse>(JsonOptions);
        Assert.NotNull(addLiftPayload);

        var createSetResponse = await client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets",
            new { reps = 5, weight = 225m });
        var createSetPayload = await createSetResponse.Content.ReadFromJsonAsync<CreateWorkoutSetResponse>(JsonOptions);
        Assert.NotNull(createSetPayload);

        var missingWorkout = await client.DeleteAsync(
            $"/api/workouts/{Guid.NewGuid()}/lifts/{addLiftPayload.WorkoutLift.Id}/sets/{createSetPayload.Set.Id}");
        var missingEntry = await client.DeleteAsync(
            $"/api/workouts/{workout.Id}/lifts/{Guid.NewGuid()}/sets/{createSetPayload.Set.Id}");

        await MarkWorkoutCompletedAsync(workout.Id);
        var conflictResponse = await client.DeleteAsync(
            $"/api/workouts/{workout.Id}/lifts/{addLiftPayload.WorkoutLift.Id}/sets/{createSetPayload.Set.Id}");

        Assert.Equal(HttpStatusCode.NotFound, missingWorkout.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingEntry.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);

        var conflictPayload = await conflictResponse.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(conflictPayload);
        Assert.Contains("Workout must be in progress to remove sets.", conflictPayload.Errors["workout"]);
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

    public sealed class CreateWorkoutSetResponse
    {
        public required Guid WorkoutId { get; init; }

        public required Guid WorkoutLiftEntryId { get; init; }

        public required WorkoutSetEntryResponse Set { get; init; }
    }

    public sealed class UpdateWorkoutSetResponse
    {
        public required Guid WorkoutId { get; init; }

        public required Guid WorkoutLiftEntryId { get; init; }

        public required WorkoutSetEntryResponse Set { get; init; }
    }

    public sealed class RemoveWorkoutLiftResponse
    {
        public required Guid WorkoutId { get; init; }

        public required Guid WorkoutLiftEntryId { get; init; }
    }

    public sealed class DeleteWorkoutSetResponse
    {
        public required Guid WorkoutId { get; init; }

        public required Guid WorkoutLiftEntryId { get; init; }

        public required Guid SetId { get; init; }
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

        public required IReadOnlyList<WorkoutSetEntryResponse> Sets { get; init; }
    }

    public sealed class WorkoutSetEntryResponse
    {
        public required Guid Id { get; init; }

        public required Guid WorkoutLiftEntryId { get; init; }

        public required int SetNumber { get; init; }

        public required int Reps { get; init; }

        public decimal? Weight { get; init; }

        public required DateTime CreatedAtUtc { get; init; }
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

    public sealed class InlineLiftHistoryResponse
    {
        public required Guid WorkoutId { get; init; }

        public required Guid WorkoutLiftEntryId { get; init; }

        public required IReadOnlyList<InlineLiftHistorySessionResponse> Items { get; init; }
    }

    public sealed class InlineLiftHistorySessionResponse
    {
        public required Guid WorkoutId { get; init; }

        public string? WorkoutLabel { get; init; }

        public required DateTime CompletedAtUtc { get; init; }

        public required IReadOnlyList<InlineLiftHistorySetResponse> Sets { get; init; }
    }

    public sealed class InlineLiftHistorySetResponse
    {
        public required int SetNumber { get; init; }

        public required int Reps { get; init; }

        public decimal? Weight { get; init; }
    }
}
