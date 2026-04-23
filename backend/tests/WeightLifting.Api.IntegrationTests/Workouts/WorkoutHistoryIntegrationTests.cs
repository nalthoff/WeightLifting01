using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Queries.ListCompletedWorkouts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class WorkoutHistoryIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task GetAsyncReturnsCompletedWorkoutsOrderedByCompletionDateDescending()
    {
        var start = new DateTime(2026, 4, 23, 12, 0, 0, DateTimeKind.Utc);
        var newestWorkoutId = Guid.NewGuid();
        var olderWorkoutId = Guid.NewGuid();
        dbContext.Workouts.AddRange(
            new WorkoutEntity
            {
                Id = olderWorkoutId,
                UserId = "default-user",
                Status = WorkoutStatus.Completed,
                Label = "Older workout",
                StartedAtUtc = start.AddHours(-2),
                CompletedAtUtc = start.AddHours(-1),
                CreatedAtUtc = start.AddHours(-2),
                UpdatedAtUtc = start.AddHours(-1),
            },
            new WorkoutEntity
            {
                Id = newestWorkoutId,
                UserId = "default-user",
                Status = WorkoutStatus.Completed,
                Label = null,
                StartedAtUtc = start.AddMinutes(-45),
                CompletedAtUtc = start.AddMinutes(-15),
                CreatedAtUtc = start.AddMinutes(-45),
                UpdatedAtUtc = start.AddMinutes(-15),
            },
            new WorkoutEntity
            {
                Id = Guid.NewGuid(),
                UserId = "default-user",
                Status = WorkoutStatus.InProgress,
                Label = "Active",
                StartedAtUtc = start,
                CreatedAtUtc = start,
                UpdatedAtUtc = start,
            },
            new WorkoutEntity
            {
                Id = Guid.NewGuid(),
                UserId = "different-user",
                Status = WorkoutStatus.Completed,
                Label = "Other user",
                StartedAtUtc = start.AddHours(-3),
                CompletedAtUtc = start.AddHours(-2),
                CreatedAtUtc = start.AddHours(-3),
                UpdatedAtUtc = start.AddHours(-2),
            });
        await dbContext.SaveChangesAsync();

        var helper = new ListCompletedWorkoutsQueryHelper(dbContext);
        var result = await helper.GetAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(newestWorkoutId, result[0].WorkoutId);
        Assert.Null(result[0].Label);
        Assert.Equal(start.AddMinutes(-15), result[0].CompletedAtUtc);
        Assert.Equal(olderWorkoutId, result[1].WorkoutId);
        Assert.Equal("Older workout", result[1].Label);
        Assert.Equal(start.AddHours(-1), result[1].CompletedAtUtc);
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
