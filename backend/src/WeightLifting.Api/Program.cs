using WeightLifting.Api.Api.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddWeightLiftingServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseExceptionHandler();
app.UseAuthorization();

app.MapControllers();

app.Run();
