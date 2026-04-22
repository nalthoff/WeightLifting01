using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Lifts.Commands.CreateLift;
using WeightLifting.Api.Application.Lifts.Commands.RenameLift;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;

namespace WeightLifting.Api.UnitTests.Application.Lifts.CreateLift;

public sealed class CreateLiftCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncThrowsWhenNameIsBlankAfterTrim()
    {
        await using var dbContext = CreateDbContext();
        var handler = new CreateLiftCommandHandler(dbContext);

        var action = () => handler.HandleAsync(new CreateLiftCommand
        {
            Name = "   ",
        }, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ArgumentException>(action);

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public async Task HandleAsyncTrimsNameBeforePersisting()
    {
        await using var dbContext = CreateDbContext();
        var handler = new CreateLiftCommandHandler(dbContext);

        var result = await handler.HandleAsync(new CreateLiftCommand
        {
            Name = "  Front Squat  ",
        }, CancellationToken.None);

        var persistedLift = await dbContext.Lifts.SingleAsync();

        Assert.Equal("Front Squat", result.Name);
        Assert.Equal("Front Squat", persistedLift.Name);
        Assert.Equal(Lift.NormalizeForUniqueLookup("Front Squat"), persistedLift.NameNormalized);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task HandleAsyncThrowsWhenNameMatchesExistingLiftCaseInsensitively()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Lifts.Add(new LiftEntity
        {
            Id = Guid.NewGuid(),
            Name = "Bench Press",
            NameNormalized = Lift.NormalizeForUniqueLookup("Bench Press"),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync();

        var handler = new CreateLiftCommandHandler(dbContext);

        var action = () => handler.HandleAsync(new CreateLiftCommand
        {
            Name = "  BENCH PRESS  ",
        }, CancellationToken.None);

        await Assert.ThrowsAsync<DuplicateLiftNameException>(action);
    }

    private static WeightLiftingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WeightLiftingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WeightLiftingDbContext(options);
    }
}
