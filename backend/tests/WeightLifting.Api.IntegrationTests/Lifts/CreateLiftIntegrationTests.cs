using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Lifts.Commands.CreateLift;
using WeightLifting.Api.Application.Lifts.Commands.RenameLift;
using WeightLifting.Api.Application.Lifts.Queries.GetLifts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.IntegrationTests.Lifts;

public sealed class CreateLiftIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task CreateLiftMakesLiftAvailableToListQueryImmediately()
    {
        var createHandler = new CreateLiftCommandHandler(dbContext);
        var getLiftsQueryHandler = new GetLiftsQueryHandler(dbContext);

        await createHandler.HandleAsync(new CreateLiftCommand
        {
            Name = "  Front Squat  ",
        }, CancellationToken.None);

        var lifts = await getLiftsQueryHandler.HandleAsync(new GetLiftsQuery(), CancellationToken.None);

        var createdLift = Assert.Single(lifts);
        Assert.Equal("Front Squat", createdLift.Name);
        Assert.True(createdLift.IsActive);
    }

    [Fact]
    public async Task CreateLiftWithDuplicateNameThrows()
    {
        var createHandler = new CreateLiftCommandHandler(dbContext);

        await createHandler.HandleAsync(new CreateLiftCommand
        {
            Name = "Bench Press",
        }, CancellationToken.None);

        var action = () => createHandler.HandleAsync(new CreateLiftCommand
        {
            Name = "bench press",
        }, CancellationToken.None);

        await Assert.ThrowsAsync<DuplicateLiftNameException>(action);
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
