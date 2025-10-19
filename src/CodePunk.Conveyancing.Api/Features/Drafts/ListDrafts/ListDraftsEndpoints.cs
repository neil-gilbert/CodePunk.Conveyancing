using CodePunk.Conveyancing.Api.Domain;
using CodePunk.Conveyancing.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.Drafts.ListDrafts;

public static class ListDraftsEndpoints
{
    public static IEndpointRouteBuilder MapListDraftsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/conveyances/{conveyanceId:guid}/drafts");

        group.MapGet("/", async (Guid conveyanceId, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var entities = await db.Drafts
                .Where(d => d.ConveyanceId == conveyanceId)
                .OrderByDescending(d => d.CreatedUtc)
                .AsNoTracking()
                .ToListAsync(ct);
            var drafts = entities.Select(ToDto).ToList();
            return Results.Ok(drafts);
        });

        return routes;
    }

    private static object ToDto(DraftDocument d) => new
    {
        d.Id,
        d.ConveyanceId,
        Type = d.Type.ToString(),
        Status = d.Status.ToString(),
        d.CreatedUtc
    };
}
