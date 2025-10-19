using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace CodePunk.Conveyancing.Api.Tests;

public class ContactsAndConveyanceContactsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public ContactsAndConveyanceContactsTests(ApiFactory factory) => _factory = factory;

    private static HttpRequestMessage WithTenant(HttpRequestMessage req, string slug)
    {
        req.Headers.Add("X-Tenant", slug);
        return req;
    }

    [Fact]
    public async Task Create_Contact_Link_To_Conveyance_And_List()
    {
        var client = _factory.CreateClient();
        var tenant = $"t{Guid.NewGuid():N}";

        // Create tenant
        (await client.PostAsJsonAsync("/api/admin/tenants", new { slug = tenant, name = tenant })).EnsureSuccessStatusCode();

        // Create contact
        var createContact = WithTenant(new HttpRequestMessage(HttpMethod.Post, "/api/contacts")
        {
            Content = JsonContent.Create(new { name = "John Buyer", email = "john@example.com", phone = "+441234567890" })
        }, tenant);
        var contactResp = await client.SendAsync(createContact);
        contactResp.StatusCode.ShouldBe(HttpStatusCode.Created);
        var contact = await contactResp.Content.ReadFromJsonAsync<JsonElement>();
        var contactId = contact.GetProperty("id").GetGuid();

        // Create conveyance
        var createCv = WithTenant(new HttpRequestMessage(HttpMethod.Post, "/api/conveyances")
        {
            Content = JsonContent.Create(new { buyerName = "Buyer A", sellerName = "Seller A", propertyAddress = "4 Test Rd" })
        }, tenant);
        var cvResp = await client.SendAsync(createCv);
        cvResp.StatusCode.ShouldBe(HttpStatusCode.Created);
        var cv = await cvResp.Content.ReadFromJsonAsync<JsonElement>();
        var conveyanceId = cv.GetProperty("id").GetGuid();

        // Link contact as Buyer
        var link = WithTenant(new HttpRequestMessage(HttpMethod.Post, $"/api/conveyances/{conveyanceId}/contacts")
        {
            Content = JsonContent.Create(new { contactId, role = 1, isClientOfTenant = true, isPrimary = true })
        }, tenant);
        var linkResp = await client.SendAsync(link);
        linkResp.StatusCode.ShouldBe(HttpStatusCode.Created);

        // List contacts for conveyance
        var list = WithTenant(new HttpRequestMessage(HttpMethod.Get, $"/api/conveyances/{conveyanceId}/contacts"), tenant);
        var listResp = await client.SendAsync(list);
        listResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var items = await listResp.Content.ReadFromJsonAsync<JsonElement>();
        items.ValueKind.ShouldBe(JsonValueKind.Array);
        items.GetArrayLength().ShouldBe(1);
        var first = items[0];
        first.GetProperty("role").GetString().ShouldBe("Buyer");
        first.GetProperty("isClientOfTenant").GetBoolean().ShouldBeTrue();
        first.GetProperty("isPrimary").GetBoolean().ShouldBeTrue();
        var c = first.GetProperty("contact");
        c.GetProperty("name").GetString().ShouldBe("John Buyer");

        // Contact 360: list conveyances for contact
        var listCv = WithTenant(new HttpRequestMessage(HttpMethod.Get, $"/api/contacts/{contactId}/conveyances"), tenant);
        var listCvResp = await client.SendAsync(listCv);
        listCvResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var cvs = await listCvResp.Content.ReadFromJsonAsync<JsonElement>();
        cvs.ValueKind.ShouldBe(JsonValueKind.Array);
        cvs.GetArrayLength().ShouldBe(1);
        cvs[0].GetProperty("id").GetGuid().ShouldBe(conveyanceId);
    }
}
