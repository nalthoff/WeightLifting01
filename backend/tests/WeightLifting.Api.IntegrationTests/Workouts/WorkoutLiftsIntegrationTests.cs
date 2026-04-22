using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.AddWorkoutLift;
using WeightLifting.Api.Application.Workouts.Queries.ListWorkoutLifts;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class WorkoutLiftsIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task AddWorkoutLiftMakesEntryAvailableToListQueryImmediately()
    {
        var workoutId = Guid.NewGuid();
        var liftId = Guid.NewGuid();
        await SeedWorkoutAndLiftsAsync(workoutId, (liftId, "Front Squat", true));

        var addHandler = new AddWorkoutLiftCommandHandler(dbContext);
        var listHelper = new ListWorkoutLiftsQueryHelper(dbContext);

        var added = await addHandler.HandleAsync(new AddWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            LiftId = liftId,
        }, CancellationToken.None);

        var listed = await listHelper.GetAsync(workoutId, CancellationToken.None);

        var only = Assert.Single(listed);
        Assert.Equal(added.WorkoutLift.Id, only.Id);
        Assert.Equal(liftId, only.LiftId);
        Assert.Equal("Front Squat", only.DisplayName);
        Assert.Equal(1, only.Position);
    }

    [Fact]
    public async Task AddWorkoutLiftAllowsDuplicateLiftEntriesAndListReturnsPositionOrder()
    {
        var workoutId = Guid.NewGuid();
        var liftId = Guid.NewGuid();
        await SeedWorkoutAndLiftsAsync(workoutId, (liftId, "Bench Press", true));

        var addHandler = new AddWorkoutLiftCommandHandler(dbContext);
        var listHelper = new ListWorkoutLiftsQueryHelper(dbContext);

        await addHandler.HandleAsync(new AddWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            LiftId = liftId,
        }, CancellationToken.None);

        await addHandler.HandleAsync(new AddWorkoutLiftCommand
        {
            WorkoutId = workoutId,
            LiftId = liftId,
        }, CancellationToken.None);

        var listed = await listHelper.GetAsync(workoutId, CancellationToken.None);

        Assert.Equal(2, listed.Count);
        Assert.Equal(1, listed[0].Position);
        Assert.Equal(2, listed[1].Position);
        Assert.All(listed, entry => Assert.Equal(liftId, entry.LiftId));
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

    private async Task SeedWorkoutAndLiftsAsync(Guid workoutId, params (Guid liftId, string name, bool isActive)[] lifts)
    {
        var timestampUtc = new DateTime(2026, 4, 22, 12, 0, 0, DateTimeKind.Utc);

        dbContext.Workouts.Add(new WorkoutEntity
        {
            Id = workoutId,
            UserId = "default-user",
            Status = WorkoutStatus.InProgress,
            Label = "Session",
            StartedAtUtc = timestampUtc,
            CreatedAtUtc = timestampUtc,
            UpdatedAtUtc = timestampUtc,
        });

        foreach (var (liftId, name, isActive) in lifts)
        {
            dbContext.Lifts.Add(new LiftEntity
            {
                Id = liftId,
                Name = name,
                NameNormalized = Lift.NormalizeForUniqueLookup(name),
                IsActive = isActive,
                CreatedAtUtc = timestampUtc,
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
