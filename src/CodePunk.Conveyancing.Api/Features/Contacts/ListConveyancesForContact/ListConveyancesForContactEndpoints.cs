using CodePunk.Conveyancing.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.Contacts.ListConveyancesForContact;

public static class ListConveyancesForContactEndpoints
{
    public static IEndpointRouteBuilder MapListConveyancesForContactEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/contacts/{contactId:guid}/conveyances");

        group.MapGet("/", async (Guid contactId, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var cvIds = await db.ConveyanceContacts.AsNoTracking()
                .Where(x => x.ContactId == contactId)
                .Select(x => x.ConveyanceId)
                .Distinct()
                .ToListAsync(ct);

            var cvs = await db.Conveyances.AsNoTracking().Where(c => cvIds.Contains(c.Id)).ToListAsync(ct);
            return Results.Ok(cvs);
        });

        return routes;
    }
}

