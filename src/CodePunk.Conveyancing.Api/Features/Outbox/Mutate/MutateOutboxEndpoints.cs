using CodePunk.Conveyancing.Api.Data;
using CodePunk.Conveyancing.Api.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.Outbox.Mutate;

public static class MutateOutboxEndpoints
{
    public sealed record EditRequest(string? Subject, string? BodyMarkdown, List<string>? ToRecipients);
    public sealed record ApproveRequest(string? ApprovedBy);

    public static IEndpointRouteBuilder MapMutateOutboxEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/outbox");

        group.MapPut("/{id:guid}", async (Guid id, EditRequest req, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var msg = await db.Outbox.FirstOrDefaultAsync(m => m.Id == id, ct);
            if (msg is null) return Results.NotFound();
            if (req.Subject is not null) msg.Subject = req.Subject;
            if (req.BodyMarkdown is not null) msg.BodyMarkdown = req.BodyMarkdown;
            if (req.ToRecipients is not null) msg.ToRecipients = req.ToRecipients;
            msg.Status = OutboxStatus.EditedByFeeEarner;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/approve", async (Guid id, ApproveRequest req, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var msg = await db.Outbox.FirstOrDefaultAsync(m => m.Id == id, ct);
            if (msg is null) return Results.NotFound();
            msg.Status = OutboxStatus.Approved;
            msg.ApprovedBy = string.IsNullOrWhiteSpace(req.ApprovedBy) ? "solicitor" : req.ApprovedBy;
            msg.ApprovedUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/send", async (Guid id, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var msg = await db.Outbox.FirstOrDefaultAsync(m => m.Id == id, ct);
            if (msg is null) return Results.NotFound();
            if (msg.Status != OutboxStatus.Approved)
            {
                return Results.BadRequest(new { message = "Message must be approved before send" });
            }
            msg.Status = OutboxStatus.Sent;
            msg.SentUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        return routes;
    }
}

