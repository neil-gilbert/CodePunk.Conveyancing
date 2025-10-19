using CodePunk.Conveyancing.Api.Data;
using CodePunk.Conveyancing.Api.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using CodePunk.Conveyancing.Api.Infrastructure.Tenancy;

namespace CodePunk.Conveyancing.Api.Features.Outbox.Create;

public static class CreateOutboxEndpoints
{
    public sealed record CreateRequest(Guid? DraftId, string? Subject, string? BodyMarkdown, List<string>? ToRecipients);

    public static IEndpointRouteBuilder MapCreateOutboxEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/conveyances/{conveyanceId:guid}/outbox");

        group.MapPost("/messages", async (Guid conveyanceId, CreateRequest req, ConveyancingDbContext db, ITenantProvider tenant, CancellationToken ct) =>
        {
            var conveyance = await db.Conveyances.AsNoTracking().AnyAsync(c => c.Id == conveyanceId, ct);
            if (!conveyance) return Results.NotFound(new { message = "Conveyance not found" });

            string subject = req.Subject ?? string.Empty;
            string body = req.BodyMarkdown ?? string.Empty;
            var to = req.ToRecipients ?? new List<string>();
            OutboxStatus status = OutboxStatus.EditedByFeeEarner;
            Guid? sourceDraftId = null;

            if (req.DraftId is Guid draftId)
            {
                var draft = await db.Drafts.AsNoTracking().FirstOrDefaultAsync(d => d.Id == draftId && d.ConveyanceId == conveyanceId, ct);
                if (draft is null) return Results.BadRequest(new { message = "Draft not found for conveyance" });
                body = string.IsNullOrWhiteSpace(body) ? draft.ContentMarkdown : body;
                status = OutboxStatus.DraftedByAgent;
                sourceDraftId = draft.Id;
                subject = string.IsNullOrWhiteSpace(subject) ? "Enquiries" : subject;
            }

            var entity = new OutboxMessage
            {
                TenantId = tenant.TenantId ?? Guid.Empty,
                Id = Guid.NewGuid(),
                ConveyanceId = conveyanceId,
                Subject = subject,
                BodyMarkdown = body,
                ToRecipients = to,
                Status = status,
                CreatedUtc = DateTime.UtcNow,
                CreatedBy = status == OutboxStatus.DraftedByAgent ? "agent" : "fee_earner",
                SourceDraftId = sourceDraftId
            };

            await db.Outbox.AddAsync(entity, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/outbox/{entity.Id}", entity);
        });

        return routes;
    }
}
