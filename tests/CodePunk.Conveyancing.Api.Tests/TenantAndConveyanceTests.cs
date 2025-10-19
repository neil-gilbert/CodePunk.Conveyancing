using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace CodePunk.Conveyancing.Api.Tests;

public class TenantAndConveyanceTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public TenantAndConveyanceTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Tenant_Provisioning_Works()
    {
        var client = _factory.CreateClient();

        var slug = $"t{Guid.NewGuid():N}";
        var create = await client.PostAsJsonAsync("/api/admin/tenants", new { slug, name = "Acme Legal" });
        new[] { HttpStatusCode.Created, HttpStatusCode.Conflict }.ShouldContain(create.StatusCode);

        var get = await client.GetAsync($"/api/admin/tenants/{slug}");
        get.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tenant = await get.Content.ReadFromJsonAsync<JsonElement>();
        tenant.GetProperty("slug").GetString().ShouldBe(slug);
    }

    [Fact]
    public async Task Conveyance_Is_Tenant_Scoped()
    {
        var client = _factory.CreateClient();

        // Create two tenants
        var acme = $"t{Guid.NewGuid():N}";
        var beta = $"t{Guid.NewGuid():N}";
        (await client.PostAsJsonAsync("/api/admin/tenants", new { slug = acme, name = "Acme" })).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync("/api/admin/tenants", new { slug = beta, name = "Beta" })).EnsureSuccessStatusCode();

        // Create conveyance under acme
        var req = new { buyerName = "Buyer A", sellerName = "Seller A", propertyAddress = "1 Test St" };
        var acmeRequest = new HttpRequestMessage(HttpMethod.Post, "/api/conveyances") { Content = JsonContent.Create(req) };
        acmeRequest.Headers.Add("X-Tenant", acme);
        var created = await client.SendAsync(acmeRequest);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("id").GetGuid();

        // Can fetch as acme
        var acmeGet = new HttpRequestMessage(HttpMethod.Get, $"/api/conveyances/{id}");
        acmeGet.Headers.Add("X-Tenant", acme);
        (await client.SendAsync(acmeGet)).StatusCode.ShouldBe(HttpStatusCode.OK);

        // Cannot fetch as beta
        var betaGet = new HttpRequestMessage(HttpMethod.Get, $"/api/conveyances/{id}");
        betaGet.Headers.Add("X-Tenant", beta);
        (await client.SendAsync(betaGet)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
