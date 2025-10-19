using CodePunk.Conveyancing.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using CodePunk.Conveyancing.Api.Domain;

namespace CodePunk.Conveyancing.Api.Features.Drafts.RejectDraft;

public static class RejectDraftEndpoints
{
    public sealed record RejectRequest(string? RejectedBy, string? Reason);

    public static IEndpointRouteBuilder MapRejectDraftEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/drafts");

        group.MapPost("/{draftId:guid}/reject", async (Guid draftId, RejectRequest req, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var draft = await db.Drafts.FirstOrDefaultAsync(d => d.Id == draftId, ct);
            if (draft is null) return Results.NotFound();
            draft.Status = DraftStatus.Rejected;
            draft.RejectedBy = string.IsNullOrWhiteSpace(req.RejectedBy) ? "solicitor" : req.RejectedBy;
            draft.RejectedUtc = DateTime.UtcNow;
            draft.RejectionReason = req.Reason;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        return routes;
    }
}
