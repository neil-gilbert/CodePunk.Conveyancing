using CodePunk.Conveyancing.Api.Data;
using CodePunk.Conveyancing.Api.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.Admin.Tenants;

public static class TenantAdminEndpoints
{
    public sealed record CreateTenantRequest(string Slug, string Name, string? Region);

    public static IEndpointRouteBuilder MapTenantAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/admin/tenants");

        group.MapGet("/", async (ConveyancingDbContext db, CancellationToken ct) =>
        {
            var tenants = await db.Tenants.AsNoTracking().OrderBy(t => t.Slug).ToListAsync(ct);
            return Results.Ok(tenants.Select(t => new { t.Id, t.Slug, t.Name, t.Region, t.Active, t.CreatedUtc }));
        });

        group.MapGet("/{slug}", async (string slug, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var t = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug.ToLower(), ct);
            return t is null ? Results.NotFound() : Results.Ok(t);
        });

        group.MapPost("/", async (CreateTenantRequest req, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var slug = (req.Slug ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(slug) || slug.Length < 2)
                return Results.BadRequest(new { error = "Slug must be at least 2 characters" });

            var exists = await db.Tenants.AnyAsync(t => t.Slug == slug, ct);
            if (exists) return Results.Conflict(new { error = "Slug already exists" });

            var entity = new Tenant
            {
                Id = Guid.NewGuid(),
                Slug = slug,
                Name = req.Name?.Trim() ?? slug,
                Region = req.Region,
                CreatedUtc = DateTime.UtcNow,
                Active = true
            };
            await db.Tenants.AddAsync(entity, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/admin/tenants/{entity.Slug}", new { entity.Id, entity.Slug, entity.Name });
        });

        return routes;
    }
}

