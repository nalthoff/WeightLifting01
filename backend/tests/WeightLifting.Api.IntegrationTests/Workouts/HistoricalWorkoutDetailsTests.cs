using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Entities;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class HistoricalWorkoutDetailsTests(HistoricalWorkoutLifecycleTests.HistoricalWorkoutWebApplicationFactory factory)
    : IClassFixture<HistoricalWorkoutLifecycleTests.HistoricalWorkoutWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task GetHistoricalWorkoutDetailsWithMinimalContentReturnsWorkoutAndNoLifts()
    {
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/workouts/historical", new
        {
            trainingDayLocalDate = new DateOnly(2026, 4, 21),
            startTimeLocal = "09:15",
            sessionLengthMinutes = 30,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("Completed", created.Workout.Status);
        Assert.Null(created.Workout.Label);

        var workoutDetailsResponse = await client.GetAsync($"/api/workouts/{created.Workout.Id}?forHistory=true");
        Assert.Equal(HttpStatusCode.OK, workoutDetailsResponse.StatusCode);
        var workoutDetails = await workoutDetailsResponse.Content.ReadFromJsonAsync<GetWorkoutResponse>(JsonOptions);
        Assert.NotNull(workoutDetails);
        Assert.Equal(created.Workout.Id, workoutDetails.Workout.Id);
        Assert.Equal(created.Workout.StartedAtUtc, workoutDetails.Workout.StartedAtUtc);
        Assert.Equal(created.Workout.CompletedAtUtc, workoutDetails.Workout.CompletedAtUtc);
        Assert.Equal(created.Workout.Label, workoutDetails.Workout.Label);
        Assert.Empty(workoutDetails.Lifts);

        var liftsResponse = await client.GetAsync($"/api/workouts/{created.Workout.Id}/lifts?forHistory=true");
        Assert.Equal(HttpStatusCode.OK, liftsResponse.StatusCode);
        var lifts = await liftsResponse.Content.ReadFromJsonAsync<WorkoutLiftListResponse>(JsonOptions);
        Assert.NotNull(lifts);
        Assert.Empty(lifts.Items);
    }

    [Fact]
    public async Task GetHistoricalWorkoutDetailsWithLiftAndSetsReturnsParityAcrossHistoryEndpoints()
    {
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/workouts/historical", new
        {
            trainingDayLocalDate = new DateOnly(2026, 4, 22),
            startTimeLocal = "05:45",
            sessionLengthMinutes = 65,
            label = "Heavy Day",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(created);

        var expectedLift = await SeedHistoricalLiftAndSetsAsync(created.Workout.Id);

        var workoutDetailsResponse = await client.GetAsync($"/api/workouts/{created.Workout.Id}?forHistory=true");
        Assert.Equal(HttpStatusCode.OK, workoutDetailsResponse.StatusCode);
        var workoutDetails = await workoutDetailsResponse.Content.ReadFromJsonAsync<GetWorkoutResponse>(JsonOptions);
        Assert.NotNull(workoutDetails);
        Assert.Equal(created.Workout.Id, workoutDetails.Workout.Id);
        Assert.Equal("Completed", workoutDetails.Workout.Status);
        Assert.Equal("Heavy Day", workoutDetails.Workout.Label);
        Assert.Equal(created.Workout.StartedAtUtc, workoutDetails.Workout.StartedAtUtc);
        Assert.Equal(created.Workout.CompletedAtUtc, workoutDetails.Workout.CompletedAtUtc);
        var detailLift = Assert.Single(workoutDetails.Lifts);
        Assert.Equal(expectedLift.Id, detailLift.Id);
        Assert.Equal(expectedLift.WorkoutId, detailLift.WorkoutId);
        Assert.Equal(expectedLift.LiftId, detailLift.LiftId);
        Assert.Equal(expectedLift.DisplayName, detailLift.DisplayName);
        Assert.Equal(expectedLift.Position, detailLift.Position);

        var liftsResponse = await client.GetAsync($"/api/workouts/{created.Workout.Id}/lifts?forHistory=true");
        Assert.Equal(HttpStatusCode.OK, liftsResponse.StatusCode);
        var lifts = await liftsResponse.Content.ReadFromJsonAsync<WorkoutLiftListResponse>(JsonOptions);
        Assert.NotNull(lifts);

        var onlyLift = Assert.Single(lifts.Items);
        Assert.Equal(expectedLift.Id, onlyLift.Id);
        Assert.Equal(expectedLift.WorkoutId, onlyLift.WorkoutId);
        Assert.Equal(expectedLift.LiftId, onlyLift.LiftId);
        Assert.Equal(expectedLift.DisplayName, onlyLift.DisplayName);
        Assert.Equal(expectedLift.Position, onlyLift.Position);
        Assert.Equal(onlyLift.Id, detailLift.Id);
        Assert.Equal(onlyLift.WorkoutId, detailLift.WorkoutId);
        Assert.Equal(onlyLift.LiftId, detailLift.LiftId);
        Assert.Equal(onlyLift.DisplayName, detailLift.DisplayName);
        Assert.Equal(onlyLift.Position, detailLift.Position);
        Assert.Equal(onlyLift.Sets.Count, detailLift.Sets.Count);

        Assert.Equal(expectedLift.Sets.Count, onlyLift.Sets.Count);
        for (var i = 0; i < expectedLift.Sets.Count; i++)
        {
            Assert.Equal(expectedLift.Sets[i].Id, onlyLift.Sets[i].Id);
            Assert.Equal(expectedLift.Sets[i].WorkoutLiftEntryId, onlyLift.Sets[i].WorkoutLiftEntryId);
            Assert.Equal(expectedLift.Sets[i].SetNumber, onlyLift.Sets[i].SetNumber);
            Assert.Equal(expectedLift.Sets[i].Reps, onlyLift.Sets[i].Reps);
            Assert.Equal(expectedLift.Sets[i].Weight, onlyLift.Sets[i].Weight);
            Assert.Equal(onlyLift.Sets[i].Id, detailLift.Sets[i].Id);
            Assert.Equal(onlyLift.Sets[i].WorkoutLiftEntryId, detailLift.Sets[i].WorkoutLiftEntryId);
            Assert.Equal(onlyLift.Sets[i].SetNumber, detailLift.Sets[i].SetNumber);
            Assert.Equal(onlyLift.Sets[i].Reps, detailLift.Sets[i].Reps);
            Assert.Equal(onlyLift.Sets[i].Weight, detailLift.Sets[i].Weight);
        }
    }

    public async Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<WorkoutLiftEntryResponse> SeedHistoricalLiftAndSetsAsync(Guid workoutId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();

        var now = new DateTime(2026, 4, 22, 11, 0, 0, DateTimeKind.Utc);
        var liftId = Guid.NewGuid();
        var workoutLiftEntryId = Guid.NewGuid();
        var firstSetId = Guid.NewGuid();
        var secondSetId = Guid.NewGuid();

        dbContext.Lifts.Add(new LiftEntity
        {
            Id = liftId,
            Name = "Back Squat",
            NameNormalized = "back squat",
            IsActive = true,
            CreatedAtUtc = now,
        });
        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = workoutLiftEntryId,
            WorkoutId = workoutId,
            LiftId = liftId,
            DisplayName = "Back Squat",
            AddedAtUtc = now,
            Position = 1,
        });
        dbContext.WorkoutSets.AddRange(
            new WorkoutSetEntity
            {
                Id = firstSetId,
                WorkoutId = workoutId,
                WorkoutLiftEntryId = workoutLiftEntryId,
                SetNumber = 1,
                Reps = 5,
                Weight = 225m,
                CreatedAtUtc = now.AddMinutes(1),
                UpdatedAtUtc = now.AddMinutes(1),
            },
            new WorkoutSetEntity
            {
                Id = secondSetId,
                WorkoutId = workoutId,
                WorkoutLiftEntryId = workoutLiftEntryId,
                SetNumber = 2,
                Reps = 3,
                Weight = null,
                CreatedAtUtc = now.AddMinutes(2),
                UpdatedAtUtc = now.AddMinutes(2),
            });

        await dbContext.SaveChangesAsync();

        return new WorkoutLiftEntryResponse
        {
            Id = workoutLiftEntryId,
            WorkoutId = workoutId,
            LiftId = liftId,
            DisplayName = "Back Squat",
            AddedAtUtc = now,
            Position = 1,
            Sets =
            [
                new WorkoutSetEntryResponse
                {
                    Id = firstSetId,
                    WorkoutLiftEntryId = workoutLiftEntryId,
                    SetNumber = 1,
                    Reps = 5,
                    Weight = 225m,
                    CreatedAtUtc = now.AddMinutes(1),
                    UpdatedAtUtc = now.AddMinutes(1),
                },
                new WorkoutSetEntryResponse
                {
                    Id = secondSetId,
                    WorkoutLiftEntryId = workoutLiftEntryId,
                    SetNumber = 2,
                    Reps = 3,
                    Weight = null,
                    CreatedAtUtc = now.AddMinutes(2),
                    UpdatedAtUtc = now.AddMinutes(2),
                },
            ],
        };
    }

    public sealed class StartWorkoutCreatedResponse
    {
        public required WorkoutSessionSummaryResponse Workout { get; init; }
    }

    public sealed class GetWorkoutResponse
    {
        public required WorkoutSessionSummaryResponse Workout { get; init; }

        public required IReadOnlyList<WorkoutLiftEntryResponse> Lifts { get; init; }
    }

    public sealed class WorkoutSessionSummaryResponse
    {
        public required Guid Id { get; init; }

        public required string Status { get; init; }

        public string? Label { get; init; }

        public required DateTime StartedAtUtc { get; init; }

        public DateTime? CompletedAtUtc { get; init; }
    }

    public sealed class WorkoutLiftListResponse
    {
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

        public required DateTime UpdatedAtUtc { get; init; }
    }
}
