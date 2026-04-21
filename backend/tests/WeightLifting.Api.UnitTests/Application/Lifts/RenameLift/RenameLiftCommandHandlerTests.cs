using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Lifts.Commands.RenameLift;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;

namespace WeightLifting.Api.UnitTests.Application.Lifts.RenameLift;

public sealed class RenameLiftCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncThrowsWhenNameIsBlankAfterTrim()
    {
        await using var dbContext = CreateDbContext();
        var liftId = await SeedLiftAsync(dbContext, "Front Squat");
        var handler = new RenameLiftCommandHandler(dbContext);

        var action = () => handler.HandleAsync(new RenameLiftCommand
        {
            LiftId = liftId,
            Name = "   ",
        }, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ArgumentException>(action);

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public async Task HandleAsyncReturnsCurrentLiftWhenNameIsUnchangedAfterTrim()
    {
        await using var dbContext = CreateDbContext();
        var liftId = await SeedLiftAsync(dbContext, "Front Squat");
        var handler = new RenameLiftCommandHandler(dbContext);

        var result = await handler.HandleAsync(new RenameLiftCommand
        {
            LiftId = liftId,
            Name = "  Front Squat  ",
        }, CancellationToken.None);

        var persistedLift = await dbContext.Lifts.SingleAsync();

        Assert.Equal("Front Squat", result.Name);
        Assert.Equal("Front Squat", persistedLift.Name);
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }

    private static async Task<Guid> SeedLiftAsync(WeightLiftingDbContext dbContext, string name)
    {
        var liftId = Guid.NewGuid();

        dbContext.Lifts.Add(new LiftEntity
        {
            Id = liftId,
            Name = name,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync();

        return liftId;
    }
}
