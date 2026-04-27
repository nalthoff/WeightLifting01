using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Queries.GetInlineLiftHistory;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Entities;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts.Queries.GetInlineLiftHistory;

public sealed class InlineLiftHistoryQueryHelperTests
{
    [Fact]
    public async Task GetAsyncReturnsExactLiftCompletedSessionsLimitedToThreeNewest()
    {
        await using var dbContext = CreateDbContext();
        var now = new DateTime(2026, 4, 27, 12, 0, 0, DateTimeKind.Utc);
        var activeWorkoutId = Guid.NewGuid();
        var activeEntryId = Guid.NewGuid();
        var exactLiftId = Guid.NewGuid();
        var otherLiftId = Guid.NewGuid();

        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = activeWorkoutId,
            UserId = "default-user",
            Status = WorkoutStatus.InProgress,
            StartedAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        });
        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = activeEntryId,
            WorkoutId = activeWorkoutId,
            LiftId = exactLiftId,
            DisplayName = "Bench Press",
            AddedAtUtc = now,
            Position = 1,
        });

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
                LiftId = exactLiftId,
                DisplayName = "Bench Press",
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

        // Unrelated lift in a newer completed workout should not appear in result.
        var unrelatedWorkoutId = Guid.NewGuid();
        var unrelatedEntryId = Guid.NewGuid();
        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = unrelatedWorkoutId,
            UserId = "default-user",
            Status = WorkoutStatus.Completed,
            Label = "Other lift",
            StartedAtUtc = now.AddHours(-2),
            CompletedAtUtc = now.AddHours(-1),
            CreatedAtUtc = now.AddHours(-2),
            UpdatedAtUtc = now.AddHours(-1),
        });
        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = unrelatedEntryId,
            WorkoutId = unrelatedWorkoutId,
            LiftId = otherLiftId,
            DisplayName = "Deadlift",
            AddedAtUtc = now.AddHours(-1),
            Position = 1,
        });

        await dbContext.SaveChangesAsync();

        var helper = new InlineLiftHistoryQueryHelper(dbContext);
        var result = await helper.GetAsync(activeWorkoutId, activeEntryId, CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.All(result, item => Assert.NotEmpty(item.Sets));
        Assert.True(result[0].CompletedAtUtc >= result[1].CompletedAtUtc);
        Assert.True(result[1].CompletedAtUtc >= result[2].CompletedAtUtc);
        Assert.DoesNotContain(result, item => item.WorkoutId == unrelatedWorkoutId);
    }

    [Fact]
    public async Task GetAsyncThrowsConflictWhenWorkoutNotInProgress()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        var now = new DateTime(2026, 4, 27, 12, 0, 0, DateTimeKind.Utc);

        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workoutId,
            UserId = "default-user",
            Status = WorkoutStatus.Completed,
            StartedAtUtc = now.AddHours(-1),
            CompletedAtUtc = now,
            CreatedAtUtc = now.AddHours(-1),
            UpdatedAtUtc = now,
        });
        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = entryId,
            WorkoutId = workoutId,
            LiftId = Guid.NewGuid(),
            DisplayName = "Bench",
            AddedAtUtc = now,
            Position = 1,
        });
        await dbContext.SaveChangesAsync();

        var helper = new InlineLiftHistoryQueryHelper(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() => helper.GetAsync(workoutId, entryId, CancellationToken.None));
    }

    [Fact]
    public async Task GetAsyncExcludesCompletedLiftSessionsWithoutLoggedSets()
    {
        await using var dbContext = CreateDbContext();
        var now = new DateTime(2026, 4, 27, 12, 0, 0, DateTimeKind.Utc);
        var activeWorkoutId = Guid.NewGuid();
        var activeEntryId = Guid.NewGuid();
        var liftId = Guid.NewGuid();

        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = activeWorkoutId,
            UserId = "default-user",
            Status = WorkoutStatus.InProgress,
            StartedAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        });
        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = activeEntryId,
            WorkoutId = activeWorkoutId,
            LiftId = liftId,
            DisplayName = "Front Squat",
            AddedAtUtc = now,
            Position = 1,
        });

        var completedWithSetsWorkoutId = Guid.NewGuid();
        var completedWithSetsEntryId = Guid.NewGuid();
        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = completedWithSetsWorkoutId,
            UserId = "default-user",
            Status = WorkoutStatus.Completed,
            Label = "With Sets",
            StartedAtUtc = now.AddDays(-1).AddHours(-1),
            CompletedAtUtc = now.AddDays(-1),
            CreatedAtUtc = now.AddDays(-1).AddHours(-1),
            UpdatedAtUtc = now.AddDays(-1),
        });
        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = completedWithSetsEntryId,
            WorkoutId = completedWithSetsWorkoutId,
            LiftId = liftId,
            DisplayName = "Front Squat",
            AddedAtUtc = now.AddDays(-1).AddMinutes(-30),
            Position = 1,
        });
        dbContext.WorkoutSets.Add(new WorkoutSetEntity
        {
            Id = Guid.NewGuid(),
            WorkoutId = completedWithSetsWorkoutId,
            WorkoutLiftEntryId = completedWithSetsEntryId,
            SetNumber = 1,
            Reps = 5,
            Weight = 185,
            CreatedAtUtc = now.AddDays(-1).AddMinutes(-10),
            UpdatedAtUtc = now.AddDays(-1).AddMinutes(-10),
        });

        var completedWithoutSetsWorkoutId = Guid.NewGuid();
        var completedWithoutSetsEntryId = Guid.NewGuid();
        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = completedWithoutSetsWorkoutId,
            UserId = "default-user",
            Status = WorkoutStatus.Completed,
            Label = "Without Sets",
            StartedAtUtc = now.AddDays(-2).AddHours(-1),
            CompletedAtUtc = now.AddDays(-2),
            CreatedAtUtc = now.AddDays(-2).AddHours(-1),
            UpdatedAtUtc = now.AddDays(-2),
        });
        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = completedWithoutSetsEntryId,
            WorkoutId = completedWithoutSetsWorkoutId,
            LiftId = liftId,
            DisplayName = "Front Squat",
            AddedAtUtc = now.AddDays(-2).AddMinutes(-30),
            Position = 1,
        });

        await dbContext.SaveChangesAsync();

        var helper = new InlineLiftHistoryQueryHelper(dbContext);
        var result = await helper.GetAsync(activeWorkoutId, activeEntryId, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(completedWithSetsWorkoutId, result[0].WorkoutId);
        Assert.DoesNotContain(result, item => item.WorkoutId == completedWithoutSetsWorkoutId);
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }
}
