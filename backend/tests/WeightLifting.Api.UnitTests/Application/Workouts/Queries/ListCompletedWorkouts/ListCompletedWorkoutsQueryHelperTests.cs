using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Queries.ListCompletedWorkouts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts.Queries.ListCompletedWorkouts;

public sealed class ListCompletedWorkoutsQueryHelperTests
{
    [Fact]
    public async Task GetAsyncReturnsCompletedOnlyOrderedNewestFirstWithDurationAndLiftCount()
    {
        await using var dbContext = CreateDbContext();
        var newerWorkoutId = Guid.NewGuid();
        var olderWorkoutId = Guid.NewGuid();
        var now = new DateTime(2026, 4, 24, 12, 0, 0, DateTimeKind.Utc);

        dbContext.Workouts.AddRange(
            new WorkoutEntity
            {
                Id = olderWorkoutId,
                UserId = "default-user",
                Status = WorkoutStatus.Completed,
                Label = "Older",
                StartedAtUtc = now.AddHours(-3),
                CompletedAtUtc = now.AddHours(-1),
                CreatedAtUtc = now.AddHours(-3),
                UpdatedAtUtc = now.AddHours(-1),
            },
            new WorkoutEntity
            {
                Id = newerWorkoutId,
                UserId = "default-user",
                Status = WorkoutStatus.Completed,
                Label = "Newer",
                StartedAtUtc = now.AddMinutes(-50),
                CompletedAtUtc = now.AddMinutes(-5),
                CreatedAtUtc = now.AddMinutes(-50),
                UpdatedAtUtc = now.AddMinutes(-5),
            },
            new WorkoutEntity
            {
                Id = Guid.NewGuid(),
                UserId = "default-user",
                Status = WorkoutStatus.InProgress,
                Label = "Ignore in progress",
                StartedAtUtc = now,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            });

        dbContext.WorkoutLiftEntries.AddRange(
            new WorkoutLiftEntryEntity
            {
                Id = Guid.NewGuid(),
                WorkoutId = newerWorkoutId,
                LiftId = Guid.NewGuid(),
                DisplayName = "Squat",
                AddedAtUtc = now.AddMinutes(-40),
                Position = 0,
            },
            new WorkoutLiftEntryEntity
            {
                Id = Guid.NewGuid(),
                WorkoutId = newerWorkoutId,
                LiftId = Guid.NewGuid(),
                DisplayName = "Bench",
                AddedAtUtc = now.AddMinutes(-35),
                Position = 1,
            },
            new WorkoutLiftEntryEntity
            {
                Id = Guid.NewGuid(),
                WorkoutId = olderWorkoutId,
                LiftId = Guid.NewGuid(),
                DisplayName = "Deadlift",
                AddedAtUtc = now.AddHours(-2),
                Position = 0,
            });

        await dbContext.SaveChangesAsync();

        var helper = new ListCompletedWorkoutsQueryHelper(dbContext);
        var result = await helper.GetAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(newerWorkoutId, result[0].WorkoutId);
        Assert.Equal("00:45", result[0].DurationDisplay);
        Assert.Equal(2, result[0].LiftCount);
        Assert.Equal(olderWorkoutId, result[1].WorkoutId);
        Assert.Equal("02:00", result[1].DurationDisplay);
        Assert.Equal(1, result[1].LiftCount);
        Assert.True(result[0].CompletedAtUtc >= result[1].CompletedAtUtc);
    }

    [Fact]
    public async Task GetAsyncWhenTimestampsInvalidUsesFallbackDuration()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var now = new DateTime(2026, 4, 24, 12, 0, 0, DateTimeKind.Utc);

        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workoutId,
            UserId = "default-user",
            Status = WorkoutStatus.Completed,
            Label = null,
            StartedAtUtc = now,
            CompletedAtUtc = now.AddMinutes(-1),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        });
        await dbContext.SaveChangesAsync();

        var helper = new ListCompletedWorkoutsQueryHelper(dbContext);
        var result = await helper.GetAsync(CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(workoutId, result[0].WorkoutId);
        Assert.Equal("00:00", result[0].DurationDisplay);
        Assert.Equal(0, result[0].LiftCount);
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }
}
