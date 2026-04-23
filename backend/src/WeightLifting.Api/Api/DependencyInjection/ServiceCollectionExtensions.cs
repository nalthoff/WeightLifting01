using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Application.Lifts.Commands.CreateLift;
using WeightLifting.Api.Application.Lifts.Commands.DeactivateLift;
using WeightLifting.Api.Application.Lifts.Commands.RenameLift;
using WeightLifting.Api.Application.Lifts.Queries.GetLifts;
using WeightLifting.Api.Application.Workouts.Commands.AddWorkoutLift;
using WeightLifting.Api.Application.Workouts.Commands.CompleteWorkout;
using WeightLifting.Api.Application.Workouts.Commands.ReorderWorkoutLifts;
using WeightLifting.Api.Application.Workouts.Commands.RemoveWorkoutLift;
using WeightLifting.Api.Application.Workouts.Commands.StartWorkout;
using WeightLifting.Api.Application.Workouts.Queries.GetActiveWorkoutSummary;
using WeightLifting.Api.Application.Workouts.Queries.GetWorkoutById;
using WeightLifting.Api.Application.Workouts.Queries.GetInProgressWorkout;
using WeightLifting.Api.Application.Workouts.Queries.ListWorkoutLifts;
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
        services.AddScoped<RenameLiftCommandHandler>();
        services.AddScoped<DeactivateLiftCommandHandler>();
        services.AddScoped<GetLiftsQueryHandler>();
        services.AddScoped<GetWorkoutByIdQueryHelper>();
        services.AddScoped<GetInProgressWorkoutQueryHelper>();
        services.AddScoped<GetActiveWorkoutSummaryQueryHelper>();
        services.AddScoped<StartWorkoutCommandHandler>();
        services.AddScoped<CompleteWorkoutCommandHandler>();
        services.AddScoped<AddWorkoutLiftCommandHandler>();
        services.AddScoped<ReorderWorkoutLiftsCommandHandler>();
        services.AddScoped<RemoveWorkoutLiftCommandHandler>();
        services.AddScoped<ListWorkoutLiftsQueryHelper>();

        return services;
    }
}
