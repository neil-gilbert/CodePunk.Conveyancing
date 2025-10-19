using CodePunk.Conveyancing.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.Contacts.List;

public static class ListContactsEndpoints
{
    public static IEndpointRouteBuilder MapListContactsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/contacts");

        group.MapGet("/", async (string? search, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var q = db.Contacts.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(c => (c.Name != null && EF.Functions.Like(c.Name, $"%{s}%"))
                              || (c.Email != null && EF.Functions.Like(c.Email, $"%{s}%"))
                              || (c.Phone != null && EF.Functions.Like(c.Phone, $"%{s}%")));
            }

            var items = await q.OrderByDescending(c => c.CreatedUtc).Take(100).ToListAsync(ct);
            return Results.Ok(items);
        });

        return routes;
    }
}

