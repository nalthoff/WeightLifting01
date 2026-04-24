using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.DeleteWorkout;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Entities;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.UnitTests.Application.Workouts.DeleteWorkout;

public sealed class DeleteWorkoutCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncDeletesWorkoutAggregateAndChildren()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        await SeedWorkoutAggregateAsync(dbContext, workoutId, WorkoutStatus.InProgress);

        var handler = new DeleteWorkoutCommandHandler(dbContext);
        var result = await handler.HandleAsync(new DeleteWorkoutCommand
        {
            WorkoutId = workoutId,
        }, CancellationToken.None);

        Assert.Equal(DeleteWorkoutOutcome.Deleted, result.Outcome);
        Assert.False(await dbContext.Workouts.AnyAsync(workout => workout.Id == workoutId));
        Assert.False(await dbContext.WorkoutLiftEntries.AnyAsync(entry => entry.WorkoutId == workoutId));
        Assert.False(await dbContext.WorkoutSets.AnyAsync(set => set.WorkoutId == workoutId));
    }

    [Fact]
    public async Task HandleAsyncReturnsNotFoundWhenWorkoutMissing()
    {
        await using var dbContext = CreateDbContext();
        var handler = new DeleteWorkoutCommandHandler(dbContext);

        var result = await handler.HandleAsync(new DeleteWorkoutCommand
        {
            WorkoutId = Guid.NewGuid(),
        }, CancellationToken.None);

        Assert.Equal(DeleteWorkoutOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task HandleAsyncReturnsConflictWhenWorkoutNotInProgress()
    {
        await using var dbContext = CreateDbContext();
        var workoutId = Guid.NewGuid();
        await SeedWorkoutAggregateAsync(dbContext, workoutId, WorkoutStatus.Completed);

        var handler = new DeleteWorkoutCommandHandler(dbContext);
        var result = await handler.HandleAsync(new DeleteWorkoutCommand
        {
            WorkoutId = workoutId,
        }, CancellationToken.None);

        Assert.Equal(DeleteWorkoutOutcome.Conflict, result.Outcome);
        Assert.True(await dbContext.Workouts.AnyAsync(workout => workout.Id == workoutId));
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }

    private static async Task SeedWorkoutAggregateAsync(
        WeightLiftingDbContext dbContext,
        Guid workoutId,
        WorkoutStatus workoutStatus)
    {
        var liftId = Guid.NewGuid();
        var workoutLiftEntryId = Guid.NewGuid();
        var setId = Guid.NewGuid();
        var timestampUtc = new DateTime(2026, 4, 24, 12, 0, 0, DateTimeKind.Utc);

        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workoutId,
            UserId = "default-user",
            Status = workoutStatus,
            Label = "Session",
            StartedAtUtc = timestampUtc,
            CompletedAtUtc = workoutStatus == WorkoutStatus.Completed ? timestampUtc.AddMinutes(30) : null,
            CreatedAtUtc = timestampUtc,
            UpdatedAtUtc = timestampUtc,
        });
        dbContext.Lifts.Add(new LiftEntity
        {
            Id = liftId,
            Name = "Bench Press",
            NameNormalized = Lift.NormalizeForUniqueLookup("Bench Press"),
            IsActive = true,
            CreatedAtUtc = timestampUtc,
        });
        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = workoutLiftEntryId,
            WorkoutId = workoutId,
            LiftId = liftId,
            DisplayName = "Bench Press",
            AddedAtUtc = timestampUtc.AddMinutes(1),
            Position = 1,
        });
        dbContext.WorkoutSets.Add(new WorkoutSetEntity
        {
            Id = setId,
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            SetNumber = 1,
            Reps = 5,
            Weight = 225m,
            CreatedAtUtc = timestampUtc.AddMinutes(2),
            UpdatedAtUtc = timestampUtc.AddMinutes(2),
        });

        await dbContext.SaveChangesAsync();
    }
}
