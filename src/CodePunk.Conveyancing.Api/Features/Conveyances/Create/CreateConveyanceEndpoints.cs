using CodePunk.Conveyancing.Api.Domain;
using CodePunk.Conveyancing.Api.Data;
using CodePunk.Conveyancing.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.Conveyances.Create;

public static class CreateConveyanceEndpoints
{
    public sealed record Request(string BuyerName, string SellerName, string PropertyAddress);

    public static IEndpointRouteBuilder MapCreateConveyanceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/conveyances");

        group.MapPost("/", async (Request req, ConveyancingDbContext db, ITenantProvider tenant, CancellationToken ct) =>
        {
            var entity = new Conveyance
            {
                TenantId = tenant.TenantId ?? Guid.Empty,
                Id = Guid.NewGuid(),
                BuyerName = req.BuyerName,
                SellerName = req.SellerName,
                PropertyAddress = req.PropertyAddress,
                CreatedUtc = DateTime.UtcNow
            };
            await db.Conveyances.AddAsync(entity, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/conveyances/{entity.Id}", entity);
        });

        return routes;
    }
}
