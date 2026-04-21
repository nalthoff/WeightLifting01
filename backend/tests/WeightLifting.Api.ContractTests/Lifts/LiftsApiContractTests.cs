using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.ContractTests.Lifts;

public sealed class LiftsApiContractTests(LiftsContractWebApplicationFactory factory)
    : IClassFixture<LiftsContractWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task PostLiftReturnsCreatedLiftContract()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = "Overhead Press",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.NotEqual(Guid.Empty, payload.Lift.Id);
        Assert.Equal("Overhead Press", payload.Lift.Name);
        Assert.True(payload.Lift.IsActive);
    }

    [Fact]
    public async Task GetLiftsReturnsListContract()
    {
        var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/lifts", new
        {
            name = "Bench Press",
        });

        var response = await client.GetAsync("/api/lifts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<LiftListResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.NotEmpty(payload.Items);
    }

    [Fact]
    public async Task PostLiftWithBlankNameReturnsValidationPayload()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = "   ",
        });

        Assert.Equal((HttpStatusCode)422, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Contains("Lift name is required.", payload.Errors["name"]);
    }

    [Fact]
    public async Task PutLiftReturnsRenamedLiftContract()
    {
        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = "Front Squat",
        });

        var createdPayload = await createResponse.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);

        Assert.NotNull(createdPayload);

        var response = await client.PutAsJsonAsync($"/api/lifts/{createdPayload.Lift.Id}", new
        {
            name = "Paused Front Squat",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<RenameLiftResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal(createdPayload.Lift.Id, payload.Lift.Id);
        Assert.Equal("Paused Front Squat", payload.Lift.Name);
        Assert.True(payload.Lift.IsActive);
    }

    [Fact]
    public async Task PutLiftWithBlankNameReturnsValidationPayload()
    {
        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = "Front Squat",
        });

        var createdPayload = await createResponse.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);

        Assert.NotNull(createdPayload);

        var response = await client.PutAsJsonAsync($"/api/lifts/{createdPayload.Lift.Id}", new
        {
            name = "   ",
        });

        Assert.Equal((HttpStatusCode)422, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Contains("Lift name is required.", payload.Errors["name"]);
    }

    [Fact]
    public async Task PutLiftWithDuplicateNameReturnsConflictPayloadAndLeavesListCanonical()
    {
        var client = factory.CreateClient();

        var firstCreateResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = "Front Squat",
        });
        var secondCreateResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = "Overhead Press",
        });

        var firstLift = await firstCreateResponse.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);
        var secondLift = await secondCreateResponse.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);

        Assert.NotNull(firstLift);
        Assert.NotNull(secondLift);

        var response = await client.PutAsJsonAsync($"/api/lifts/{firstLift.Lift.Id}", new
        {
            name = "  OVERHEAD PRESS  ",
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Contains("Lift name already exists.", payload.Errors["name"]);

        var listResponse = await client.GetAsync("/api/lifts");
        var listPayload = await listResponse.Content.ReadFromJsonAsync<LiftListResponse>(JsonOptions);

        Assert.NotNull(listPayload);
        Assert.Contains(listPayload.Items, item => item.Id == firstLift.Lift.Id && item.Name == "Front Squat");
        Assert.Contains(listPayload.Items, item => item.Id == secondLift.Lift.Id && item.Name == "Overhead Press");
    }

    public sealed class CreateLiftResponse
    {
        public required LiftResponse Lift { get; init; }
    }

    public sealed class LiftResponse
    {
        public required Guid Id { get; init; }

        public required string Name { get; init; }

        public required bool IsActive { get; init; }
    }

    public sealed class RenameLiftResponse
    {
        public required LiftResponse Lift { get; init; }
    }

    public sealed class LiftListResponse
    {
        public required IReadOnlyList<LiftListItemResponse> Items { get; init; }
    }

    public sealed class LiftListItemResponse
    {
        public required Guid Id { get; init; }

        public required string Name { get; init; }

        public required bool IsActive { get; init; }
    }

    public sealed class ValidationErrorResponse
    {
        public required Dictionary<string, string[]> Errors { get; init; }
    }
}

public sealed class LiftsContractWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = "Sqlite",
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={databasePath}",
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var client = CreateClient();
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WeightLiftingDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public new Task DisposeAsync()
    {
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }

        return Task.CompletedTask;
    }
}
