using Microsoft.Extensions.Configuration;

namespace WeightLifting.Api.ContractTests;

/// <summary>
/// Ensures contract / integration-style tests never resolve to SQL Server or the developer LocalDB database.
/// </summary>
public static class ContractTestDatabaseGuard
{
    public static void EnsureIsolatedSqlite(IConfiguration configuration)
    {
        var provider = configuration["Persistence:Provider"];
        if (!string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Contract tests must use Persistence:Provider=Sqlite (resolved: '{provider ?? "(null)"}'). " +
                "Refusing to run to avoid writing test data to the developer database.");
        }

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Contract tests require ConnectionStrings:DefaultConnection to be set to a SQLite data source.");
        }

        if (!connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Contract tests must use a SQLite connection string (missing 'Data Source=').");
        }

        if (connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
            || connectionString.Contains("MSSQLLocalDB", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Contract tests must not use LocalDB. Check configuration source order and environment variables.");
        }

        if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Contract tests must not use a SQL Server-style connection string (found 'Server=').");
        }

        if (connectionString.Contains("Database=WeightLifting01", StringComparison.OrdinalIgnoreCase)
            || connectionString.Contains("Initial Catalog=WeightLifting01", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Contract tests must not target the WeightLifting01 database.");
        }
    }
}
