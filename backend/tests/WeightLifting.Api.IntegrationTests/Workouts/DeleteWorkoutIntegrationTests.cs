using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.DeleteWorkout;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Entities;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class DeleteWorkoutIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task DeleteWorkoutRemovesWorkoutAggregateAndChildRows()
    {
        var workoutId = Guid.NewGuid();
        await SeedWorkoutAggregateAsync(workoutId, WorkoutStatus.InProgress);
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
    public async Task DeleteWorkoutReturnsExpectedNotFoundAndConflictOutcomes()
    {
        var completedWorkoutId = Guid.NewGuid();
        await SeedWorkoutAggregateAsync(completedWorkoutId, WorkoutStatus.Completed);
        var handler = new DeleteWorkoutCommandHandler(dbContext);

        var notFound = await handler.HandleAsync(new DeleteWorkoutCommand
        {
            WorkoutId = Guid.NewGuid(),
        }, CancellationToken.None);
        var conflict = await handler.HandleAsync(new DeleteWorkoutCommand
        {
            WorkoutId = completedWorkoutId,
        }, CancellationToken.None);

        Assert.Equal(DeleteWorkoutOutcome.NotFound, notFound.Outcome);
        Assert.Equal(DeleteWorkoutOutcome.Conflict, conflict.Outcome);
        Assert.True(await dbContext.Workouts.AnyAsync(workout => workout.Id == completedWorkoutId));
    }

    [Fact]
    public async Task DeleteWorkoutStaleStateReturnsConflictWhenWorkoutCompletedBeforeDelete()
    {
        var workoutId = Guid.NewGuid();
        await SeedWorkoutAggregateAsync(workoutId, WorkoutStatus.InProgress);

        var workoutEntity = await dbContext.Workouts.SingleAsync(workout => workout.Id == workoutId);
        workoutEntity.Status = WorkoutStatus.Completed;
        workoutEntity.CompletedAtUtc = workoutEntity.StartedAtUtc.AddMinutes(45);
        workoutEntity.UpdatedAtUtc = workoutEntity.CompletedAtUtc.Value;
        await dbContext.SaveChangesAsync();

        var handler = new DeleteWorkoutCommandHandler(dbContext);
        var result = await handler.HandleAsync(new DeleteWorkoutCommand
        {
            WorkoutId = workoutId,
        }, CancellationToken.None);

        Assert.Equal(DeleteWorkoutOutcome.Conflict, result.Outcome);
        Assert.True(await dbContext.Workouts.AnyAsync(workout => workout.Id == workoutId));
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

    private async Task SeedWorkoutAggregateAsync(Guid workoutId, WorkoutStatus workoutStatus)
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
            Name = "Squat",
            NameNormalized = Lift.NormalizeForUniqueLookup("Squat"),
            IsActive = true,
            CreatedAtUtc = timestampUtc,
        });
        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = workoutLiftEntryId,
            WorkoutId = workoutId,
            LiftId = liftId,
            DisplayName = "Squat",
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
            Weight = 315m,
            CreatedAtUtc = timestampUtc.AddMinutes(2),
            UpdatedAtUtc = timestampUtc.AddMinutes(2),
        });

        await dbContext.SaveChangesAsync();
    }
}
