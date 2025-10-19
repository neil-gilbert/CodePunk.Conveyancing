using CodePunk.Conveyancing.Api.Data;
using CodePunk.Conveyancing.Api.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.ConveyanceContacts.List;

public static class ListConveyanceContactsEndpoints
{
    public static IEndpointRouteBuilder MapListConveyanceContactsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/conveyances/{conveyanceId:guid}/contacts");

        group.MapGet("/", async (Guid conveyanceId, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var links = await db.ConveyanceContacts.AsNoTracking().Where(x => x.ConveyanceId == conveyanceId).ToListAsync(ct);
            var contactIds = links.Select(l => l.ContactId).Distinct().ToList();
            var contacts = await db.Contacts.AsNoTracking().Where(c => contactIds.Contains(c.Id)).ToListAsync(ct);
            var byId = contacts.ToDictionary(c => c.Id);

            var result = links.Select(l => new
            {
                l.Id,
                l.ConveyanceId,
                Contact = byId.TryGetValue(l.ContactId, out var c) ? new { c.Id, c.Name, c.Email, c.Phone } : null,
                Role = l.Role.ToString(),
                l.IsClientOfTenant,
                l.IsPrimary,
                l.CreatedUtc
            });

            return Results.Ok(result);
        });

        return routes;
    }
}

