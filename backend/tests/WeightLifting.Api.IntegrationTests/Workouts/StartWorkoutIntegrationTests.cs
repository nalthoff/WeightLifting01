using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Workouts.Commands.StartWorkout;
using WeightLifting.Api.Application.Workouts.Queries.GetInProgressWorkout;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.IntegrationTests.Workouts;

public sealed class StartWorkoutIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task StartWorkoutMakesCreatedWorkoutAvailableToInProgressQueryImmediately()
    {
        var queryHelper = new GetInProgressWorkoutQueryHelper(dbContext);
        var handler = new StartWorkoutCommandHandler(dbContext, queryHelper);

        var result = await handler.HandleAsync(new StartWorkoutCommand
        {
            Label = "  Front Squat Session  ",
        }, CancellationToken.None);

        var inProgressWorkout = await queryHelper.GetAsync("default-user", CancellationToken.None);

        Assert.Equal(StartWorkoutOutcome.Created, result.Outcome);
        Assert.NotNull(inProgressWorkout);
        Assert.Equal(result.Workout.Id, inProgressWorkout.Id);
        Assert.Equal("Front Squat Session", inProgressWorkout.Label);
        Assert.Equal(WorkoutStatus.InProgress, inProgressWorkout.Status);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData("  PM Session  ", "PM Session")]
    public async Task StartWorkoutPersistsNormalizedOptionalLabel(string? inputLabel, string? expectedPersistedLabel)
    {
        var queryHelper = new GetInProgressWorkoutQueryHelper(dbContext);
        var handler = new StartWorkoutCommandHandler(dbContext, queryHelper);

        var result = await handler.HandleAsync(new StartWorkoutCommand
        {
            Label = inputLabel,
        }, CancellationToken.None);

        var persistedWorkout = await dbContext.Workouts.SingleAsync(workout => workout.Id == result.Workout.Id);

        Assert.Equal(expectedPersistedLabel, persistedWorkout.Label);
    }

    [Fact]
    public async Task StartWorkoutWhenAlreadyInProgressReturnsConflictOutcomeAndDoesNotCreateSecondWorkout()
    {
        var queryHelper = new GetInProgressWorkoutQueryHelper(dbContext);
        var handler = new StartWorkoutCommandHandler(dbContext, queryHelper);

        var firstResult = await handler.HandleAsync(new StartWorkoutCommand
        {
            Label = "Session A",
        }, CancellationToken.None);

        var secondResult = await handler.HandleAsync(new StartWorkoutCommand
        {
            Label = "Session B",
        }, CancellationToken.None);

        var workouts = await dbContext.Workouts.OrderBy(workout => workout.CreatedAtUtc).ToListAsync();

        Assert.Equal(StartWorkoutOutcome.Created, firstResult.Outcome);
        Assert.Equal(StartWorkoutOutcome.AlreadyInProgress, secondResult.Outcome);
        Assert.Single(workouts);
        Assert.Equal(firstResult.Workout.Id, workouts[0].Id);
        Assert.Equal(firstResult.Workout.Id, secondResult.Workout.Id);
        Assert.Equal("Session A", workouts[0].Label);
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
