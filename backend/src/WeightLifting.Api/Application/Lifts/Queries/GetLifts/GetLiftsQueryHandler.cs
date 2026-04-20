using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Lifts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Lifts.Queries.GetLifts;

public sealed class GetLiftsQueryHandler(WeightLiftingDbContext dbContext)
{
    public async Task<IReadOnlyList<Lift>> HandleAsync(GetLiftsQuery query, CancellationToken cancellationToken)
    {
        var liftQuery = dbContext.Lifts.AsNoTracking();

        if (query.ActiveOnly)
        {
            liftQuery = liftQuery.Where(lift => lift.IsActive);
        }

        return await liftQuery
            .OrderBy(lift => lift.Name)
            .Select(lift => new Lift(lift.Id, lift.Name, lift.IsActive, lift.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
