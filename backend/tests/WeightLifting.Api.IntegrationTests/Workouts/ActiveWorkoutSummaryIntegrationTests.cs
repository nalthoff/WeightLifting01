using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Queries.GetActiveWorkoutSummary;
using WeightLifting.Api.Application.Workouts.Queries.GetInProgressWorkout;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class ActiveWorkoutSummaryIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task GetAsyncReturnsNullWhenNoInProgressWorkoutExists()
    {
        var helper = CreateHelper();

        var result = await helper.GetAsync(CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsyncReturnsInProgressWorkoutSummary()
    {
        var startedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);
        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = Guid.NewGuid(),
            UserId = "default-user",
            Status = WorkoutStatus.InProgress,
            Label = "Bench Session",
            StartedAtUtc = startedAtUtc,
            CreatedAtUtc = startedAtUtc,
            UpdatedAtUtc = startedAtUtc,
        });
        await dbContext.SaveChangesAsync();

        var helper = CreateHelper();
        var result = await helper.GetAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Bench Session", result.Label);
        Assert.Equal(WorkoutStatus.InProgress, result.Status);
        Assert.Null(result.CompletedAtUtc);
    }

    private GetActiveWorkoutSummaryQueryHelper CreateHelper()
    {
        var inProgressHelper = new GetInProgressWorkoutQueryHelper(dbContext);
        return new GetActiveWorkoutSummaryQueryHelper(inProgressHelper);
    }

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseSqlite(connection)
            .Options;

        dbContext = new WeightLiftingDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await dbContext.DisposeAsync();
        await connection.DisposeAsync();
    }
}
