using CodePunk.Conveyancing.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.Contacts.Get;

public static class GetContactEndpoints
{
    public static IEndpointRouteBuilder MapGetContactEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/contacts");

        group.MapGet("/{id:guid}", async (Guid id, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var c = await db.Contacts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            return c is null ? Results.NotFound() : Results.Ok(c);
        });

        return routes;
    }
}

