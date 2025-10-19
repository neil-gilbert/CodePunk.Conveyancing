using CodePunk.Conveyancing.Api.Data;
using CodePunk.Conveyancing.Api.Domain;
using CodePunk.Conveyancing.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.ConveyanceContacts.Add;

public static class AddConveyanceContactEndpoints
{
    public sealed record Request(Guid ContactId, ConveyanceContactRole Role, bool IsClientOfTenant, bool IsPrimary);

    public static IEndpointRouteBuilder MapAddConveyanceContactEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/conveyances/{conveyanceId:guid}/contacts");

        group.MapPost("/", async (Guid conveyanceId, Request req, ConveyancingDbContext db, ITenantProvider tenant, CancellationToken ct) =>
        {
            var conveyance = await db.Conveyances.AsNoTracking().FirstOrDefaultAsync(c => c.Id == conveyanceId, ct);
            if (conveyance is null) return Results.NotFound(new { message = "Conveyance not found" });

            var contact = await db.Contacts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == req.ContactId, ct);
            if (contact is null) return Results.BadRequest(new { message = "Contact not found" });

            var entity = new ConveyanceContact
            {
                TenantId = tenant.TenantId ?? Guid.Empty,
                Id = Guid.NewGuid(),
                ConveyanceId = conveyanceId,
                ContactId = req.ContactId,
                Role = req.Role,
                IsClientOfTenant = req.IsClientOfTenant,
                IsPrimary = req.IsPrimary,
                CreatedUtc = DateTime.UtcNow
            };

            await db.ConveyanceContacts.AddAsync(entity, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/conveyances/{conveyanceId}/contacts/{entity.Id}", entity);
        });

        return routes;
    }
}

