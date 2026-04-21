using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Lifts.Commands.CreateLift;
using WeightLifting.Api.Application.Lifts.Commands.RenameLift;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.IntegrationTests.Lifts;

public sealed class RenameLiftIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task RenameLiftUpdatesExistingLiftWithoutCreatingAnotherRecord()
    {
        var createHandler = new CreateLiftCommandHandler(dbContext);
        var renameHandler = new RenameLiftCommandHandler(dbContext);

        var createdLift = await createHandler.HandleAsync(new CreateLiftCommand
        {
            Name = "Front Squat",
        }, CancellationToken.None);

        var renamedLift = await renameHandler.HandleAsync(new RenameLiftCommand
        {
            LiftId = createdLift.Id,
            Name = "Paused Front Squat",
        }, CancellationToken.None);

        var persistedLifts = await dbContext.Lifts.ToListAsync();
        var persistedLift = Assert.Single(persistedLifts);

        Assert.Equal(createdLift.Id, renamedLift.Id);
        Assert.Equal("Paused Front Squat", renamedLift.Name);
        Assert.Equal("Paused Front Squat", persistedLift.Name);
    }

    [Fact]
    public async Task RenameLiftWithBlankNameLeavesPersistedLiftUnchanged()
    {
        var createHandler = new CreateLiftCommandHandler(dbContext);
        var renameHandler = new RenameLiftCommandHandler(dbContext);

        var createdLift = await createHandler.HandleAsync(new CreateLiftCommand
        {
            Name = "Front Squat",
        }, CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(() => renameHandler.HandleAsync(new RenameLiftCommand
        {
            LiftId = createdLift.Id,
            Name = "   ",
        }, CancellationToken.None));

        var persistedLift = await dbContext.Lifts.SingleAsync();

        Assert.Equal("Front Squat", persistedLift.Name);
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
