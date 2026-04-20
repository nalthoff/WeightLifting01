using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace WeightLifting.Api.Api.ProblemDetails;

public static class ProblemDetailsConfiguration
{
    public static IServiceCollection AddWeightLiftingProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                context.ProblemDetails.Extensions["path"] = context.HttpContext.Request.Path.Value;
            };
        });

        return services;
    }
}
