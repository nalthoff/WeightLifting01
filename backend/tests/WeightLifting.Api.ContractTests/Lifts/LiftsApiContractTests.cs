using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WeightLifting.Api.ContractTests;

namespace WeightLifting.Api.ContractTests.Lifts;

public sealed class LiftsApiContractTests(LiftsContractWebApplicationFactory factory)
    : IClassFixture<LiftsContractWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Prefix makes accidental leakage into a dev database obvious; guard prevents LocalDB use.</summary>
    private static string UniqueLiftName(string label) => $"[contract] {label} {Guid.NewGuid():N}";

    [Fact]
    public async Task PostLiftReturnsCreatedLiftContract()
    {
        var client = factory.CreateClient();
        var liftName = UniqueLiftName("Overhead Press");

        var response = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = liftName,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.NotEqual(Guid.Empty, payload.Lift.Id);
        Assert.Equal(liftName, payload.Lift.Name);
        Assert.True(payload.Lift.IsActive);
    }

    [Fact]
    public async Task GetLiftsReturnsListContract()
    {
        var client = factory.CreateClient();
        var liftName = UniqueLiftName("Bench Press");

        await client.PostAsJsonAsync("/api/lifts", new
        {
            name = liftName,
        });

        var response = await client.GetAsync("/api/lifts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<LiftListResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Contains(payload.Items, item => item.Name == liftName);
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
    public async Task PostLiftWithDuplicateNameReturnsConflictPayload()
    {
        var client = factory.CreateClient();
        var liftName = UniqueLiftName("Bench Press");

        var firstResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = liftName,
        });
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var duplicateResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = $"  {liftName.ToUpperInvariant()}  ",
        });

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var payload = await duplicateResponse.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Contains("Lift name already exists.", payload.Errors["name"]);
    }

    [Fact]
    public async Task PutLiftReturnsRenamedLiftContract()
    {
        var client = factory.CreateClient();
        var originalName = UniqueLiftName("Front Squat");
        var renamed = UniqueLiftName("Paused Front Squat");

        var createResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = originalName,
        });

        var createdPayload = await createResponse.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);

        Assert.NotNull(createdPayload);

        var response = await client.PutAsJsonAsync($"/api/lifts/{createdPayload.Lift.Id}", new
        {
            name = renamed,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<RenameLiftResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal(createdPayload.Lift.Id, payload.Lift.Id);
        Assert.Equal(renamed, payload.Lift.Name);
        Assert.True(payload.Lift.IsActive);
    }

    [Fact]
    public async Task PutLiftWithBlankNameReturnsValidationPayload()
    {
        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = UniqueLiftName("Front Squat"),
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
        var token = Guid.NewGuid().ToString("N");
        var firstName = $"[contract] Front Squat {token}";
        var secondName = $"[contract] Overhead Press {token}";

        var firstCreateResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = firstName,
        });
        var secondCreateResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = secondName,
        });

        var firstLift = await firstCreateResponse.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);
        var secondLift = await secondCreateResponse.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);

        Assert.NotNull(firstLift);
        Assert.NotNull(secondLift);

        var response = await client.PutAsJsonAsync($"/api/lifts/{firstLift.Lift.Id}", new
        {
            name = $"  {secondName.ToUpperInvariant()}  ",
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Contains("Lift name already exists.", payload.Errors["name"]);

        var listResponse = await client.GetAsync("/api/lifts");
        var listPayload = await listResponse.Content.ReadFromJsonAsync<LiftListResponse>(JsonOptions);

        Assert.NotNull(listPayload);
        Assert.Contains(listPayload.Items, item => item.Id == firstLift.Lift.Id && item.Name == firstName);
        Assert.Contains(listPayload.Items, item => item.Id == secondLift.Lift.Id && item.Name == secondName);
    }

    [Fact]
    public async Task PutLiftDeactivateReturnsDeactivatedLiftContract()
    {
        var client = factory.CreateClient();
        var liftName = UniqueLiftName("Front Squat");

        var createResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = liftName,
        });

        var createdPayload = await createResponse.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);

        Assert.NotNull(createdPayload);

        var response = await client.PutAsync($"/api/lifts/{createdPayload.Lift.Id}/deactivate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<DeactivateLiftResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Equal(createdPayload.Lift.Id, payload.Lift.Id);
        Assert.Equal(liftName, payload.Lift.Name);
        Assert.False(payload.Lift.IsActive);
    }

    [Fact]
    public async Task PutLiftDeactivateWithMissingLiftReturnsNotFound()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsync($"/api/lifts/{Guid.NewGuid()}/deactivate", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLiftsActiveOnlyFilterExcludesInactiveWhenTrueAndIncludesInactiveWhenFalse()
    {
        var client = factory.CreateClient();
        var firstName = UniqueLiftName("Front Squat");
        var secondName = UniqueLiftName("Overhead Press");

        var firstCreateResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = firstName,
        });
        var secondCreateResponse = await client.PostAsJsonAsync("/api/lifts", new
        {
            name = secondName,
        });

        var firstLift = await firstCreateResponse.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);
        var secondLift = await secondCreateResponse.Content.ReadFromJsonAsync<CreateLiftResponse>(JsonOptions);

        Assert.NotNull(firstLift);
        Assert.NotNull(secondLift);

        var deactivateResponse = await client.PutAsync($"/api/lifts/{firstLift.Lift.Id}/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var activeOnlyResponse = await client.GetAsync("/api/lifts?activeOnly=true");
        var activeOnlyPayload = await activeOnlyResponse.Content.ReadFromJsonAsync<LiftListResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, activeOnlyResponse.StatusCode);
        Assert.NotNull(activeOnlyPayload);
        Assert.DoesNotContain(activeOnlyPayload.Items, item => item.Id == firstLift.Lift.Id);
        Assert.Contains(activeOnlyPayload.Items, item => item.Id == secondLift.Lift.Id && item.IsActive);

        var includeInactiveResponse = await client.GetAsync("/api/lifts?activeOnly=false");
        var includeInactivePayload = await includeInactiveResponse.Content.ReadFromJsonAsync<LiftListResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, includeInactiveResponse.StatusCode);
        Assert.NotNull(includeInactivePayload);
        Assert.Contains(includeInactivePayload.Items, item => item.Id == firstLift.Lift.Id && !item.IsActive);
        Assert.Contains(includeInactivePayload.Items, item => item.Id == secondLift.Lift.Id && item.IsActive);
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

    public sealed class DeactivateLiftResponse
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
