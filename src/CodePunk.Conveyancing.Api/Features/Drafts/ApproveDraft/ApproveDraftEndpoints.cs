using CodePunk.Conveyancing.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using CodePunk.Conveyancing.Api.Domain;

namespace CodePunk.Conveyancing.Api.Features.Drafts.ApproveDraft;

public static class ApproveDraftEndpoints
{
    public sealed record ApproveRequest(string ApprovedBy);

    public static IEndpointRouteBuilder MapApproveDraftEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/drafts");

        group.MapPost("/{draftId:guid}/approve", async (Guid draftId, ApproveRequest req, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var draft = await db.Drafts.FirstOrDefaultAsync(d => d.Id == draftId, ct);
            if (draft is null) return Results.NotFound();
            draft.Status = DraftStatus.Approved;
            draft.ApprovedBy = string.IsNullOrWhiteSpace(req.ApprovedBy) ? "solicitor" : req.ApprovedBy;
            draft.ApprovedUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        return routes;
    }
}
