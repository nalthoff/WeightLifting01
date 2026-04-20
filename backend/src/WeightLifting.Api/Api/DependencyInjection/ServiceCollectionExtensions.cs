using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Lifts.Commands.CreateLift;
using WeightLifting.Api.Application.Lifts.Queries.GetLifts;
using WeightLifting.Api.Api.ProblemDetails;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeightLiftingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var persistenceProvider = configuration["Persistence:Provider"] ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=WeightLifting01;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddWeightLiftingProblemDetails();

        services.AddDbContext<WeightLiftingDbContext>(options =>
        {
            if (string.Equals(persistenceProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
                return;
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<CreateLiftCommandHandler>();
        services.AddScoped<GetLiftsQueryHandler>();

        return services;
    }
}
