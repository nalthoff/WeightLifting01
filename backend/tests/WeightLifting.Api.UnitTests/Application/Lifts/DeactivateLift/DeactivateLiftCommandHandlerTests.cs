using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Lifts.Commands.DeactivateLift;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;

namespace WeightLifting.Api.UnitTests.Application.Lifts.DeactivateLift;

public sealed class DeactivateLiftCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncDeactivatesActiveLift()
    {
        await using var dbContext = CreateDbContext();
        var liftId = await SeedLiftAsync(dbContext, "Front Squat", isActive: true);
        var handler = new DeactivateLiftCommandHandler(dbContext);

        var result = await handler.HandleAsync(new DeactivateLiftCommand
        {
            LiftId = liftId,
        }, CancellationToken.None);

        var persistedLift = await dbContext.Lifts.SingleAsync(lift => lift.Id == liftId);

        Assert.False(result.IsActive);
        Assert.False(persistedLift.IsActive);
    }

    [Fact]
    public async Task HandleAsyncThrowsWhenLiftIsMissing()
    {
        await using var dbContext = CreateDbContext();
        var handler = new DeactivateLiftCommandHandler(dbContext);

        var action = () => handler.HandleAsync(new DeactivateLiftCommand
        {
            LiftId = Guid.NewGuid(),
        }, CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task HandleAsyncReturnsCurrentLiftWhenAlreadyInactive()
    {
        await using var dbContext = CreateDbContext();
        var liftId = await SeedLiftAsync(dbContext, "Front Squat", isActive: false);
        var handler = new DeactivateLiftCommandHandler(dbContext);

        var result = await handler.HandleAsync(new DeactivateLiftCommand
        {
            LiftId = liftId,
        }, CancellationToken.None);

        var persistedLift = await dbContext.Lifts.SingleAsync(lift => lift.Id == liftId);

        Assert.False(result.IsActive);
        Assert.False(persistedLift.IsActive);
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }

    private static async Task<Guid> SeedLiftAsync(WeightLiftingDbContext dbContext, string name, bool isActive)
    {
        var liftId = Guid.NewGuid();

        dbContext.Lifts.Add(new LiftEntity
        {
            Id = liftId,
            Name = name,
            NameNormalized = Lift.NormalizeForUniqueLookup(name),
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync();

        return liftId;
    }
}
