using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Lifts.Commands.DeactivateLift;

public sealed class DeactivateLiftCommandHandler(WeightLiftingDbContext dbContext)
{
    public async Task<Lift> HandleAsync(DeactivateLiftCommand command, CancellationToken cancellationToken)
    {
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

        var deactivatedLift = currentLift.Deactivate();

        if (!currentLift.IsActive)
        {
            return currentLift;
        }

        liftEntity.IsActive = deactivatedLift.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new Lift(liftEntity.Id, liftEntity.Name, liftEntity.IsActive, liftEntity.CreatedAtUtc);
    }
}
