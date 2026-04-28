using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class HistoricalWorkoutLifecycleTests(HistoricalWorkoutLifecycleTests.HistoricalWorkoutWebApplicationFactory factory)
    : IClassFixture<HistoricalWorkoutLifecycleTests.HistoricalWorkoutWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task PostHistoricalWorkoutCreatesCompletedWorkoutWithExpectedTimestampsAndHistoryCompatibility()
    {
        var client = factory.CreateClient();
        var trainingDay = new DateOnly(2026, 4, 19);
        const string startTimeLocal = "06:45";
        const int sessionLengthMinutes = 75;
        var expectedStartedAtUtc = new DateTime(2026, 4, 19, 6, 45, 0, DateTimeKind.Utc);
        var expectedCompletedAtUtc = expectedStartedAtUtc.AddMinutes(sessionLengthMinutes);

        var createResponse = await client.PostAsJsonAsync("/api/workouts/historical", new
        {
            trainingDayLocalDate = trainingDay,
            startTimeLocal,
            sessionLengthMinutes,
            label = "Backfilled Session",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdPayload = await createResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(createdPayload);
        Assert.NotEqual(Guid.Empty, createdPayload.Workout.Id);
        Assert.Equal("Completed", createdPayload.Workout.Status);
        Assert.Equal(expectedStartedAtUtc, createdPayload.Workout.StartedAtUtc);
        Assert.Equal(expectedCompletedAtUtc, createdPayload.Workout.CompletedAtUtc);

        var getForHistoryResponse = await client.GetAsync($"/api/workouts/{createdPayload.Workout.Id}?forHistory=true");
        Assert.Equal(HttpStatusCode.OK, getForHistoryResponse.StatusCode);
        var getForHistoryPayload = await getForHistoryResponse.Content.ReadFromJsonAsync<GetWorkoutResponse>(JsonOptions);
        Assert.NotNull(getForHistoryPayload);
        Assert.Equal(createdPayload.Workout.Id, getForHistoryPayload.Workout.Id);
        Assert.Equal("Completed", getForHistoryPayload.Workout.Status);
        Assert.Equal(expectedStartedAtUtc, getForHistoryPayload.Workout.StartedAtUtc);
        Assert.Equal(expectedCompletedAtUtc, getForHistoryPayload.Workout.CompletedAtUtc);

        var historyResponse = await client.GetAsync("/api/workouts/history");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);
        var historyPayload = await historyResponse.Content.ReadFromJsonAsync<WorkoutHistoryResponse>(JsonOptions);
        Assert.NotNull(historyPayload);
        var item = Assert.Single(historyPayload.Items);
        Assert.Equal(createdPayload.Workout.Id, item.WorkoutId);
        Assert.Equal("Backfilled Session", item.Label);
        Assert.Equal(expectedCompletedAtUtc, item.CompletedAtUtc);
        Assert.Equal("01:15", item.DurationDisplay);
    }

    [Fact]
    public async Task PostCompleteOnHistoricalWorkoutReturnsConflict()
    {
        var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/workouts/historical", new
        {
            trainingDayLocalDate = new DateOnly(2026, 4, 20),
            startTimeLocal = "07:00",
            sessionLengthMinutes = 40,
            label = "Already Completed",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdPayload = await createResponse.Content.ReadFromJsonAsync<StartWorkoutCreatedResponse>(JsonOptions);
        Assert.NotNull(createdPayload);

        var completeResponse = await client.PostAsync($"/api/workouts/{createdPayload.Workout.Id}/complete", null);
        Assert.Equal(HttpStatusCode.Conflict, completeResponse.StatusCode);
        var payload = await completeResponse.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Workout cannot be completed", payload.Title);
        Assert.Equal((int)HttpStatusCode.Conflict, payload.Status);
        Assert.Contains("Workout must be in progress to complete.", payload.Errors["workout"]);
    }

    public async Task InitializeAsync()
    {
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        HistoricalTestDatabaseGuard.EnsureIsolatedSqlite(configuration);
        var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public sealed class HistoricalWorkoutWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            var sqliteConnection = $"Data Source={databasePath}";
            builder.UseSetting("Persistence:Provider", "Sqlite");
            builder.UseSetting("ConnectionStrings:DefaultConnection", sqliteConnection);
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Persistence:Provider"] = "Sqlite",
                    ["ConnectionStrings:DefaultConnection"] = sqliteConnection,
                });
            });
        }

        public async Task InitializeAsync()
        {
            using var client = CreateClient();
            using var scope = Services.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            HistoricalTestDatabaseGuard.EnsureIsolatedSqlite(configuration);

            var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }

        public new Task DisposeAsync()
        {
            TryDeleteSqliteFiles(databasePath);
            return Task.CompletedTask;
        }

        private static void TryDeleteSqliteFiles(string dbPath)
        {
            foreach (var path in new[] { dbPath, $"{dbPath}-wal", $"{dbPath}-shm" })
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch (IOException)
                {
                    // File may still be open briefly after the host disposes.
                }
            }
        }
    }

    private static class HistoricalTestDatabaseGuard
    {
        public static void EnsureIsolatedSqlite(IConfiguration configuration)
        {
            var provider = configuration["Persistence:Provider"];
            if (!string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Integration tests must use Persistence:Provider=Sqlite (resolved: '{provider ?? "(null)"}').");
            }

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Integration tests require ConnectionStrings:DefaultConnection to be set.");
            }

            if (!connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Integration tests must use a SQLite connection string (missing 'Data Source=').");
            }
        }
    }

    public sealed class StartWorkoutCreatedResponse
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
