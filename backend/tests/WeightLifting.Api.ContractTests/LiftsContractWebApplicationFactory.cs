using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.ContractTests;

public sealed class LiftsContractWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        var sqliteConnection = $"Data Source={databasePath}";
        builder.UseSetting("Persistence:Provider", "Sqlite");
        builder.UseSetting("ConnectionStrings:DefaultConnection", sqliteConnection);
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = "Sqlite",
                ["ConnectionStrings:DefaultConnection"] = sqliteConnection,
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var client = CreateClient();
        using var scope = Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        ContractTestDatabaseGuard.EnsureIsolatedSqlite(configuration);

        var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public new Task DisposeAsync()
    {
        TryDeleteSqliteFiles(databasePath);
        return Task.CompletedTask;
    }

    private static void TryDeleteSqliteFiles(string dbPath)
    {
        foreach (var path in new[] { dbPath, $"{dbPath}-wal", $"{dbPath}-shm" })
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
                // File may still be open briefly after the host disposes.
            }
        }
    }
}
