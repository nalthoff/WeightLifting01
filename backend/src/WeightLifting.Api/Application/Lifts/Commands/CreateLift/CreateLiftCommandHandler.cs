using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;

namespace WeightLifting.Api.Application.Lifts.Commands.CreateLift;

public sealed class CreateLiftCommandHandler(WeightLiftingDbContext dbContext)
{
    public async Task<Lift> HandleAsync(CreateLiftCommand command, CancellationToken cancellationToken)
    {
        var lift = new Lift(Guid.NewGuid(), command.Name, true, DateTime.UtcNow);

        dbContext.Lifts.Add(new LiftEntity
        {
            Id = lift.Id,
            Name = lift.Name,
            IsActive = lift.IsActive,
            CreatedAtUtc = lift.CreatedAtUtc,
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return lift;
    }
}
