using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.RemoveWorkoutLift;
using WeightLifting.Api.Application.Workouts.Queries.ListWorkoutLifts;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class RemoveWorkoutLiftIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task RemoveWorkoutLiftDeletesEntryAndPersistsListState()
    {
        var workoutId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        await SeedWorkoutAsync(workoutId, WorkoutStatus.InProgress);
        await SeedEntryAsync(entryId, workoutId, Guid.NewGuid(), "Front Squat", 1);

        var commandHandler = new RemoveWorkoutLiftCommandHandler(dbContext);
        var listHelper = new ListWorkoutLiftsQueryHelper(dbContext);

        var result = await commandHandler.HandleAsync(new RemoveWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = entryId,
        }, CancellationToken.None);

        var listed = await listHelper.GetAsync(workoutId, CancellationToken.None);

        Assert.Equal(RemoveWorkoutLiftOutcome.Removed, result.Outcome);
        Assert.Equal(entryId, result.WorkoutLiftEntryId);
        Assert.Empty(listed);
        Assert.False(await dbContext.WorkoutLiftEntries.AnyAsync(entity => entity.Id == entryId));
    }

    [Fact]
    public async Task RemoveWorkoutLiftWithDuplicatesRemovesOnlyTargetedEntry()
    {
        var workoutId = Guid.NewGuid();
        var sharedLiftId = Guid.NewGuid();
        var firstEntryId = Guid.NewGuid();
        var secondEntryId = Guid.NewGuid();
        await SeedWorkoutAsync(workoutId, WorkoutStatus.InProgress);
        await SeedEntryAsync(firstEntryId, workoutId, sharedLiftId, "Bench Press", 1);
        await SeedEntryAsync(secondEntryId, workoutId, sharedLiftId, "Bench Press", 2);

        var commandHandler = new RemoveWorkoutLiftCommandHandler(dbContext);
        var listHelper = new ListWorkoutLiftsQueryHelper(dbContext);

        var result = await commandHandler.HandleAsync(new RemoveWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = secondEntryId,
        }, CancellationToken.None);

        var listed = await listHelper.GetAsync(workoutId, CancellationToken.None);

        var remaining = Assert.Single(listed);
        Assert.Equal(RemoveWorkoutLiftOutcome.Removed, result.Outcome);
        Assert.Equal(secondEntryId, result.WorkoutLiftEntryId);
        Assert.Equal(firstEntryId, remaining.Id);
        Assert.Equal(sharedLiftId, remaining.LiftId);
        Assert.Equal(1, remaining.Position);
    }

    [Fact]
    public async Task RemoveWorkoutLiftReturnsNotFoundForMissingWorkoutOrEntry()
    {
        var workoutId = Guid.NewGuid();
        var existingEntryId = Guid.NewGuid();
        await SeedWorkoutAsync(workoutId, WorkoutStatus.InProgress);
        await SeedEntryAsync(existingEntryId, workoutId, Guid.NewGuid(), "Row", 1);

        var commandHandler = new RemoveWorkoutLiftCommandHandler(dbContext);

        var missingWorkoutResult = await commandHandler.HandleAsync(new RemoveWorkoutLiftCommand
        {
            WorkoutId = Guid.NewGuid(),
            WorkoutLiftEntryId = existingEntryId,
        }, CancellationToken.None);

        var missingEntryResult = await commandHandler.HandleAsync(new RemoveWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = Guid.NewGuid(),
        }, CancellationToken.None);

        Assert.Equal(RemoveWorkoutLiftOutcome.NotFound, missingWorkoutResult.Outcome);
        Assert.Equal(RemoveWorkoutLiftOutcome.NotFound, missingEntryResult.Outcome);
    }

    [Fact]
    public async Task RemoveWorkoutLiftReturnsConflictWhenWorkoutNotInProgress()
    {
        var workoutId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        await SeedWorkoutAsync(workoutId, WorkoutStatus.Completed);
        await SeedEntryAsync(entryId, workoutId, Guid.NewGuid(), "Deadlift", 1);

        var commandHandler = new RemoveWorkoutLiftCommandHandler(dbContext);

        var result = await commandHandler.HandleAsync(new RemoveWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = entryId,
        }, CancellationToken.None);

        Assert.Equal(RemoveWorkoutLiftOutcome.Conflict, result.Outcome);
        Assert.True(await dbContext.WorkoutLiftEntries.AnyAsync(entity => entity.Id == entryId));
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

    private async Task SeedWorkoutAsync(Guid workoutId, WorkoutStatus status)
    {
        var timestampUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);
        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workoutId,
            UserId = "default-user",
            Status = status,
            Label = "Session",
            StartedAtUtc = timestampUtc,
            CompletedAtUtc = status == WorkoutStatus.Completed ? timestampUtc.AddMinutes(30) : null,
            CreatedAtUtc = timestampUtc,
            UpdatedAtUtc = timestampUtc,
        });

        await dbContext.SaveChangesAsync();
    }

    private async Task SeedEntryAsync(
        Guid entryId,
        Guid workoutId,
        Guid liftId,
        string displayName,
        int position)
    {
        var existingLift = await dbContext.Lifts.SingleOrDefaultAsync(entity => entity.Id == liftId);
        if (existingLift is null)
        {
            dbContext.Lifts.Add(new LiftEntity
            {
                Id = liftId,
                Name = displayName,
                NameNormalized = Lift.NormalizeForUniqueLookup(displayName),
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc),
            });
        }

        dbContext.WorkoutLiftEntries.Add(new WorkoutLiftEntryEntity
        {
            Id = entryId,
            WorkoutId = workoutId,
            LiftId = liftId,
            DisplayName = displayName,
            AddedAtUtc = new DateTime(2026, 4, 22, 12, 5, 0, DateTimeKind.Utc).AddMinutes(position),
            Position = position,
        });

        await dbContext.SaveChangesAsync();
    }
}
