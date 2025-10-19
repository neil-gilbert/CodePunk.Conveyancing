using CodePunk.Conveyancing.Api.Data;
using CodePunk.Conveyancing.Api.Domain;
using CodePunk.Conveyancing.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.Contacts.Create;

public static class CreateContactEndpoints
{
    public sealed record Request(string Name, string? Email, string? Phone);

    public static IEndpointRouteBuilder MapCreateContactEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/contacts");

        group.MapPost("/", async (Request req, ConveyancingDbContext db, ITenantProvider tenant, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["name"] = ["Name is required"] });

            var entity = new Contact
            {
                TenantId = tenant.TenantId ?? Guid.Empty,
                Id = Guid.NewGuid(),
                Name = req.Name.Trim(),
                Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email,
                Phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone,
                CreatedUtc = DateTime.UtcNow
            };

            await db.Contacts.AddAsync(entity, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/contacts/{entity.Id}", entity);
        });

        return routes;
    }
}

