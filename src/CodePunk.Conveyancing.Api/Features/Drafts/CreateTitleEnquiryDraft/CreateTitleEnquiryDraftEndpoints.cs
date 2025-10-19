using CodePunk.Conveyancing.Api.Agents;
using CodePunk.Conveyancing.Api.Domain;
using CodePunk.Conveyancing.Api.Infrastructure;
using CodePunk.Conveyancing.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using CodePunk.Conveyancing.Api.Infrastructure.Tenancy;

namespace CodePunk.Conveyancing.Api.Features.Drafts.CreateTitleEnquiryDraft;

public static class CreateTitleEnquiryDraftEndpoints
{
    public static IEndpointRouteBuilder MapCreateTitleEnquiryDraftEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/conveyances/{conveyanceId:guid}/drafts/title-enquiries");

        group.MapPost("/", async (Guid conveyanceId, ITitleEnquiryAgent agent, ConveyancingDbContext db, IConfiguration config, ITenantProvider tenant, CancellationToken ct) =>
        {
            var conveyance = await db.Conveyances.FirstOrDefaultAsync(c => c.Id == conveyanceId, ct);
            if (conveyance is null) return Results.NotFound(new { message = "Conveyance not found" });

            var result = await agent.DraftEnquiriesAsync(conveyance.PropertyAddress, ct);
            var content = result.Content;
            var meta = new Dictionary<string, string>(result.Audit)
            {
                ["source"] = "agents",
                ["type"] = nameof(DraftType.TitleEnquiries)
            };
            var draft = new DraftDocument
            {
                TenantId = tenant.TenantId ?? Guid.Empty,
                Id = Guid.NewGuid(),
                ConveyanceId = conveyanceId,
                Type = DraftType.TitleEnquiries,
                Status = DraftStatus.Draft,
                ContentMarkdown = content,
                CreatedUtc = DateTime.UtcNow,
                Metadata = meta
            };
            db.Drafts.Add(draft);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/drafts/{draft.Id}", ToDto(draft));
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
