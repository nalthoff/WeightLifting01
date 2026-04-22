using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Lifts.Commands.RenameLift;

public sealed class RenameLiftCommandHandler(WeightLiftingDbContext dbContext)
{
    public async Task<Lift> HandleAsync(RenameLiftCommand command, CancellationToken cancellationToken)
    {
        var normalizedRequestedName = Lift.NormalizeName(command.Name);
        var normalizedRequestedNameLower = normalizedRequestedName.ToLowerInvariant();

        var liftEntity = await dbContext.Lifts
            .SingleOrDefaultAsync(lift => lift.Id == command.LiftId, cancellationToken);

        if (liftEntity is null)
        {
            throw new KeyNotFoundException("Lift was not found.");
        }

        var currentLift = new Lift(
            liftEntity.Id,
            liftEntity.Name,
            liftEntity.IsActive,
            liftEntity.CreatedAtUtc);

        var renamedLift = currentLift.Rename(command.Name);

        if (string.Equals(renamedLift.Name, currentLift.Name, StringComparison.Ordinal))
        {
            return currentLift;
        }

        var hasDuplicateName = await dbContext.Lifts.AnyAsync(lift =>
            lift.Id != command.LiftId
            && lift.NameNormalized == normalizedRequestedNameLower,
            cancellationToken);

        if (hasDuplicateName)
        {
            throw new DuplicateLiftNameException(normalizedRequestedName);
        }

        liftEntity.Name = normalizedRequestedName;
        liftEntity.NameNormalized = normalizedRequestedNameLower;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            throw new DuplicateLiftNameException(normalizedRequestedName);
        }

        return new Lift(liftEntity.Id, liftEntity.Name, liftEntity.IsActive, liftEntity.CreatedAtUtc);
    }
}
