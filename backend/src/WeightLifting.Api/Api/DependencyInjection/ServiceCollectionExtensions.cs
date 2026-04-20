using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Api.ProblemDetails;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeightLiftingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=WeightLifting01;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddWeightLiftingProblemDetails();

        services.AddDbContext<WeightLiftingDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        return services;
    }
}
