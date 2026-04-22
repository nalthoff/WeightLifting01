using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Lifts.Commands.CreateLift;
using WeightLifting.Api.Application.Lifts.Commands.DeactivateLift;
using WeightLifting.Api.Application.Lifts.Queries.GetLifts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.IntegrationTests.Lifts;

public sealed class DeactivateLiftIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WeightLiftingDbContext dbContext = null!;

    [Fact]
    public async Task DeactivateLiftPersistsInactiveState()
    {
        var createHandler = new CreateLiftCommandHandler(dbContext);
        var deactivateHandler = new DeactivateLiftCommandHandler(dbContext);

        var createdLift = await createHandler.HandleAsync(new CreateLiftCommand
        {
            Name = "Front Squat",
        }, CancellationToken.None);

        var deactivatedLift = await deactivateHandler.HandleAsync(new DeactivateLiftCommand
        {
            LiftId = createdLift.Id,
        }, CancellationToken.None);

        var persistedLift = await dbContext.Lifts.SingleAsync(lift => lift.Id == createdLift.Id);

        Assert.False(deactivatedLift.IsActive);
        Assert.False(persistedLift.IsActive);
    }

    [Fact]
    public async Task GetLiftsWithActiveOnlyFiltersOutDeactivatedLifts()
    {
        var createHandler = new CreateLiftCommandHandler(dbContext);
        var deactivateHandler = new DeactivateLiftCommandHandler(dbContext);
        var listHandler = new GetLiftsQueryHandler(dbContext);

        var frontSquat = await createHandler.HandleAsync(new CreateLiftCommand
        {
            Name = "Front Squat",
        }, CancellationToken.None);

        var overheadPress = await createHandler.HandleAsync(new CreateLiftCommand
        {
            Name = "Overhead Press",
        }, CancellationToken.None);

        await deactivateHandler.HandleAsync(new DeactivateLiftCommand
        {
            LiftId = frontSquat.Id,
        }, CancellationToken.None);

        var activeOnlyLifts = await listHandler.HandleAsync(new GetLiftsQuery
        {
            ActiveOnly = true,
        }, CancellationToken.None);

        var includeInactiveLifts = await listHandler.HandleAsync(new GetLiftsQuery
        {
            ActiveOnly = false,
        }, CancellationToken.None);

        Assert.DoesNotContain(activeOnlyLifts, lift => lift.Id == frontSquat.Id);
        Assert.Contains(activeOnlyLifts, lift => lift.Id == overheadPress.Id);

        Assert.Contains(includeInactiveLifts, lift => lift.Id == frontSquat.Id && !lift.IsActive);
        Assert.Contains(includeInactiveLifts, lift => lift.Id == overheadPress.Id && lift.IsActive);
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
