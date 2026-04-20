using WeightLifting.Api.Api.DependencyInjection;
using WeightLifting.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddWeightLiftingServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsEnvironment("Test"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.

app.UseExceptionHandler();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
