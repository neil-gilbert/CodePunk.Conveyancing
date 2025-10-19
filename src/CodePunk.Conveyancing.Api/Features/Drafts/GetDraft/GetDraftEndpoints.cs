using CodePunk.Conveyancing.Api.Domain;
using CodePunk.Conveyancing.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.Drafts.GetDraft;

public static class GetDraftEndpoints
{
    public static IEndpointRouteBuilder MapGetDraftEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/drafts");

        group.MapGet("/{draftId:guid}", async (Guid draftId, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var draft = await db.Drafts.FirstOrDefaultAsync(d => d.Id == draftId, ct);
            return draft is not null ? Results.Ok(ToDto(draft)) : Results.NotFound();
        });

        return routes;
    }

    private static object ToDto(DraftDocument d) => new
    {
        d.Id,
        d.ConveyanceId,
        Type = d.Type.ToString(),
        Status = d.Status.ToString(),
        d.ContentMarkdown,
        d.CreatedUtc,
        d.ApprovedUtc,
        d.RejectedUtc,
        d.RejectionReason,
        Metadata = d.Metadata
    };
}
