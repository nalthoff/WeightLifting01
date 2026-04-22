using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Lifts.Commands.RenameLift;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Infrastructure.Persistence;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;

namespace WeightLifting.Api.Application.Lifts.Commands.CreateLift;

public sealed class CreateLiftCommandHandler(WeightLiftingDbContext dbContext)
{
    public async Task<Lift> HandleAsync(CreateLiftCommand command, CancellationToken cancellationToken)
    {
        var lift = new Lift(Guid.NewGuid(), command.Name, true, DateTime.UtcNow);
        var nameKey = Lift.NormalizeForUniqueLookup(lift.Name);

        var hasDuplicateName = await dbContext.Lifts.AnyAsync(
            entity => entity.NameNormalized == nameKey,
            cancellationToken);

        if (hasDuplicateName)
        {
            throw new DuplicateLiftNameException(lift.Name);
        }

        dbContext.Lifts.Add(new LiftEntity
        {
            Id = lift.Id,
            Name = lift.Name,
            NameNormalized = nameKey,
            IsActive = lift.IsActive,
            CreatedAtUtc = lift.CreatedAtUtc,
        });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            throw new DuplicateLiftNameException(lift.Name);
        }

        return lift;
    }
}
