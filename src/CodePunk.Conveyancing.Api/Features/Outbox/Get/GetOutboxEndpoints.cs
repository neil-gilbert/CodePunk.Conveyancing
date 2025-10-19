using CodePunk.Conveyancing.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.Outbox.Get;

public static class GetOutboxEndpoints
{
    public static IEndpointRouteBuilder MapGetOutboxEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/outbox");

        group.MapGet("/{id:guid}", async (Guid id, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var msg = await db.Outbox.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);
            return msg is null ? Results.NotFound() : Results.Ok(msg);
        });

        return routes;
    }
}

