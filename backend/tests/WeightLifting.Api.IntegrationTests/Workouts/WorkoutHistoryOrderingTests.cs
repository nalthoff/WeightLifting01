using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class WorkoutHistoryOrderingTests(HistoricalWorkoutLifecycleTests.HistoricalWorkoutWebApplicationFactory factory)
    : IClassFixture<HistoricalWorkoutLifecycleTests.HistoricalWorkoutWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task GetWorkoutHistoryWhenCompletionTimesTieOrdersByStartThenIdDescendingDeterministically()
    {
        using var client = factory.CreateClient();

        var firstCreateResponse = await client.PostAsJsonAsync("/api/workouts/historical", new
        {
            trainingDayLocalDate = new DateOnly(2026, 4, 24),
            startTimeLocal = "06:00",
            sessionLengthMinutes = 60,
            label = "FirstCreatedEarlierStart",
        });
        Assert.Equal(HttpStatusCode.Created, firstCreateResponse.StatusCode);
        var firstPayload = await firstCreateResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(firstPayload);

        var secondCreateResponse = await client.PostAsJsonAsync("/api/workouts/historical", new
        {
            trainingDayLocalDate = new DateOnly(2026, 4, 24),
            startTimeLocal = "06:15",
            sessionLengthMinutes = 45,
            label = "SecondCreatedLaterStart",
        });
        Assert.Equal(HttpStatusCode.Created, secondCreateResponse.StatusCode);
        var secondPayload = await secondCreateResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(secondPayload);

        var thirdCreateResponse = await client.PostAsJsonAsync("/api/workouts/historical", new
        {
            trainingDayLocalDate = new DateOnly(2026, 4, 24),
            startTimeLocal = "06:15",
            sessionLengthMinutes = 45,
            label = "ThirdCreatedSameStart",
        });
        Assert.Equal(HttpStatusCode.Created, thirdCreateResponse.StatusCode);
        var thirdPayload = await thirdCreateResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(thirdPayload);

        var firstHistoryResponse = await client.GetAsync("/api/workouts/history");
        Assert.Equal(HttpStatusCode.OK, firstHistoryResponse.StatusCode);
        var firstHistoryPayload = await firstHistoryResponse.Content.ReadFromJsonAsync<WorkoutHistoryResponse>(JsonOptions);
        Assert.NotNull(firstHistoryPayload);

        var firstOrder = firstHistoryPayload.Items
            .Take(3)
            .Select(item => item.WorkoutId)
            .ToArray();

        var expectedTopId = secondPayload.Workout.Id.CompareTo(thirdPayload.Workout.Id) > 0
            ? secondPayload.Workout.Id
            : thirdPayload.Workout.Id;
        var expectedSecondId = expectedTopId == secondPayload.Workout.Id
            ? thirdPayload.Workout.Id
            : secondPayload.Workout.Id;

        Assert.Equal(
            [expectedTopId, expectedSecondId, firstPayload.Workout.Id],
            firstOrder);

        var secondHistoryResponse = await client.GetAsync("/api/workouts/history");
        Assert.Equal(HttpStatusCode.OK, secondHistoryResponse.StatusCode);
        var secondHistoryPayload = await secondHistoryResponse.Content.ReadFromJsonAsync<WorkoutHistoryResponse>(JsonOptions);
        Assert.NotNull(secondHistoryPayload);

        var secondOrder = secondHistoryPayload.Items
            .Take(3)
            .Select(item => item.WorkoutId)
            .ToArray();

        Assert.Equal(firstOrder, secondOrder);
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
    }

    public sealed class WorkoutHistoryResponse
    {
        public required IReadOnlyList<WorkoutHistoryItemResponse> Items { get; init; }
    }

    public sealed class WorkoutHistoryItemResponse
    {
        public required Guid WorkoutId { get; init; }
    }
}
