using CodePunk.Conveyancing.Api.Data;
using CodePunk.Conveyancing.Api.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.Outbox.List;

public static class ListOutboxEndpoints
{
    public static IEndpointRouteBuilder MapListOutboxEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/conveyances/{conveyanceId:guid}/outbox");

        group.MapGet("/", async (Guid conveyanceId, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var items = await db.Outbox
                .Where(o => o.ConveyanceId == conveyanceId)
                .OrderByDescending(o => o.CreatedUtc)
                .AsNoTracking()
                .ToListAsync(ct);
            return Results.Ok(items.Select(ToSummary));
        });

        return routes;
    }

    private static object ToSummary(OutboxMessage o) => new
    {
        o.Id,
        o.ConveyanceId,
        o.Subject,
        o.Status,
        o.CreatedUtc,
        o.ApprovedUtc,
        o.SentUtc
    };
}

