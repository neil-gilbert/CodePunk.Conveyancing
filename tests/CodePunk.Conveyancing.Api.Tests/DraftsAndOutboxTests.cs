using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace CodePunk.Conveyancing.Api.Tests;

public class DraftsAndOutboxTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public DraftsAndOutboxTests(ApiFactory factory) => _factory = factory;

    private async Task CreateTenantAsync(HttpClient client, string slug)
    {
        var resp = await client.PostAsJsonAsync("/api/admin/tenants", new { slug, name = slug });
        if (resp.StatusCode is not HttpStatusCode.Created && resp.StatusCode is not HttpStatusCode.Conflict)
        {
            resp.EnsureSuccessStatusCode();
        }
    }

    private static HttpRequestMessage WithTenant(HttpRequestMessage req, string slug)
    {
        req.Headers.Add("X-Tenant", slug);
        return req;
    }

    [Fact]
    public async Task Create_Draft_Then_List()
    {
        var client = _factory.CreateClient();
        var tenant = $"t{Guid.NewGuid():N}";
        await CreateTenantAsync(client, tenant);

        // Create conveyance
        var createCv = WithTenant(new HttpRequestMessage(HttpMethod.Post, "/api/conveyances")
        {
            Content = JsonContent.Create(new { buyerName = "Buyer", sellerName = "Seller", propertyAddress = "10 Downing St" })
        }, tenant);
        var createdCv = await client.SendAsync(createCv);
        createdCv.EnsureSuccessStatusCode();
        var cv = await createdCv.Content.ReadFromJsonAsync<JsonElement>();
        var id = cv.GetProperty("id").GetGuid();

        // Create draft title enquiries
        var createDraft = WithTenant(new HttpRequestMessage(HttpMethod.Post, $"/api/conveyances/{id}/drafts/title-enquiries"), tenant);
        var createdDraft = await client.SendAsync(createDraft);
        createdDraft.StatusCode.ShouldBe(HttpStatusCode.Created);

        // List drafts
        var listDrafts = WithTenant(new HttpRequestMessage(HttpMethod.Get, $"/api/conveyances/{id}/drafts"), tenant);
        var listResp = await client.SendAsync(listDrafts);
        listResp.EnsureSuccessStatusCode();
        var list = await listResp.Content.ReadFromJsonAsync<JsonElement>();
        list.ValueKind.ShouldBe(JsonValueKind.Array);
        list.GetArrayLength().ShouldBe(1);
    }

    [Fact]
    public async Task Outbox_From_Draft_Approve_Send()
    {
        var client = _factory.CreateClient();
        var tenant = $"t{Guid.NewGuid():N}";
        await CreateTenantAsync(client, tenant);

        // Create conveyance
        var createCv = WithTenant(new HttpRequestMessage(HttpMethod.Post, "/api/conveyances")
        {
            Content = JsonContent.Create(new { buyerName = "B", sellerName = "S", propertyAddress = "1 Test Rd" })
        }, tenant);
        var createdCv = await client.SendAsync(createCv);
        createdCv.EnsureSuccessStatusCode();
        var cv = await createdCv.Content.ReadFromJsonAsync<JsonElement>();
        var conveyanceId = cv.GetProperty("id").GetGuid();

        // Create draft
        var draftResp = await client.SendAsync(WithTenant(new HttpRequestMessage(HttpMethod.Post, $"/api/conveyances/{conveyanceId}/drafts/title-enquiries"), tenant));
        draftResp.EnsureSuccessStatusCode();
        var draft = await draftResp.Content.ReadFromJsonAsync<JsonElement>();
        var draftId = draft.GetProperty("id").GetGuid();

        // Create outbox message from draft
        var createMsg = WithTenant(new HttpRequestMessage(HttpMethod.Post, $"/api/conveyances/{conveyanceId}/outbox/messages")
        {
            Content = JsonContent.Create(new { draftId, toRecipients = new[] { "seller.sol@firm.example" }, subject = "Enquiries" })
        }, tenant);
        var msgResp = await client.SendAsync(createMsg);
        msgResp.StatusCode.ShouldBe(HttpStatusCode.Created);
        var msg = await msgResp.Content.ReadFromJsonAsync<JsonElement>();
        var msgId = msg.GetProperty("id").GetGuid();

        // Approve
        var approve = WithTenant(new HttpRequestMessage(HttpMethod.Post, $"/api/outbox/{msgId}/approve")
        {
            Content = JsonContent.Create(new { approvedBy = "fee.earner@firm" })
        }, tenant);
        (await client.SendAsync(approve)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Send
        var send = WithTenant(new HttpRequestMessage(HttpMethod.Post, $"/api/outbox/{msgId}/send"), tenant);
        (await client.SendAsync(send)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Draft_Approve_And_Reject_Update_Status()
    {
        var client = _factory.CreateClient();
        var tenant = $"t{Guid.NewGuid():N}";
        await CreateTenantAsync(client, tenant);

        // Create conveyance
        var createCv = WithTenant(new HttpRequestMessage(HttpMethod.Post, "/api/conveyances")
        {
            Content = JsonContent.Create(new { buyerName = "B", sellerName = "S", propertyAddress = "2 Test Rd" })
        }, tenant);
        var createdCv = await client.SendAsync(createCv);
        createdCv.EnsureSuccessStatusCode();
        var cv = await createdCv.Content.ReadFromJsonAsync<JsonElement>();
        var conveyanceId = cv.GetProperty("id").GetGuid();

        // Draft A
        var draftRespA = await client.SendAsync(WithTenant(new HttpRequestMessage(HttpMethod.Post, $"/api/conveyances/{conveyanceId}/drafts/title-enquiries"), tenant));
        draftRespA.EnsureSuccessStatusCode();
        var draftA = await draftRespA.Content.ReadFromJsonAsync<JsonElement>();
        var draftAId = draftA.GetProperty("id").GetGuid();

        // Approve A
        var approve = WithTenant(new HttpRequestMessage(HttpMethod.Post, $"/api/drafts/{draftAId}/approve")
        {
            Content = JsonContent.Create(new { approvedBy = "fee.earner@firm" })
        }, tenant);
        (await client.SendAsync(approve)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify status Approved
        var getA = WithTenant(new HttpRequestMessage(HttpMethod.Get, $"/api/drafts/{draftAId}"), tenant);
        var getAResp = await client.SendAsync(getA);
        getAResp.EnsureSuccessStatusCode();
        var a = await getAResp.Content.ReadFromJsonAsync<JsonElement>();
        a.GetProperty("status").GetString().ShouldBe("Approved");

        // Draft B
        var draftRespB = await client.SendAsync(WithTenant(new HttpRequestMessage(HttpMethod.Post, $"/api/conveyances/{conveyanceId}/drafts/title-enquiries"), tenant));
        draftRespB.EnsureSuccessStatusCode();
        var draftB = await draftRespB.Content.ReadFromJsonAsync<JsonElement>();
        var draftBId = draftB.GetProperty("id").GetGuid();

        // Reject B
        var reject = WithTenant(new HttpRequestMessage(HttpMethod.Post, $"/api/drafts/{draftBId}/reject")
        {
            Content = JsonContent.Create(new { rejectedBy = "fee.earner@firm", reason = "Contains errors" })
        }, tenant);
        (await client.SendAsync(reject)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify status Rejected
        var getB = WithTenant(new HttpRequestMessage(HttpMethod.Get, $"/api/drafts/{draftBId}"), tenant);
        var getBResp = await client.SendAsync(getB);
        getBResp.EnsureSuccessStatusCode();
        var b = await getBResp.Content.ReadFromJsonAsync<JsonElement>();
        b.GetProperty("status").GetString().ShouldBe("Rejected");
    }

    [Fact]
    public async Task Outbox_Send_Requires_Approval()
    {
        var client = _factory.CreateClient();
        var tenant = $"t{Guid.NewGuid():N}";
        await CreateTenantAsync(client, tenant);

        // Create conveyance
        var createCv = WithTenant(new HttpRequestMessage(HttpMethod.Post, "/api/conveyances")
        {
            Content = JsonContent.Create(new { buyerName = "B", sellerName = "S", propertyAddress = "3 Test Rd" })
        }, tenant);
        var createdCv = await client.SendAsync(createCv);
        createdCv.EnsureSuccessStatusCode();
        var cv = await createdCv.Content.ReadFromJsonAsync<JsonElement>();
        var conveyanceId = cv.GetProperty("id").GetGuid();

        // Create outbox message directly (not approved)
        var createMsg = WithTenant(new HttpRequestMessage(HttpMethod.Post, $"/api/conveyances/{conveyanceId}/outbox/messages")
        {
            Content = JsonContent.Create(new { subject = "Hello", bodyMarkdown = "Body", toRecipients = new[] { "someone@example.com" } })
        }, tenant);
        var msgResp = await client.SendAsync(createMsg);
        msgResp.StatusCode.ShouldBe(HttpStatusCode.Created);
        var msg = await msgResp.Content.ReadFromJsonAsync<JsonElement>();
        var msgId = msg.GetProperty("id").GetGuid();

        // Send should fail with 400
        var send = WithTenant(new HttpRequestMessage(HttpMethod.Post, $"/api/outbox/{msgId}/send"), tenant);
        var sendResp = await client.SendAsync(send);
        sendResp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var err = await sendResp.Content.ReadFromJsonAsync<JsonElement>();
        err.TryGetProperty("message", out _).ShouldBeTrue();
    }
}
