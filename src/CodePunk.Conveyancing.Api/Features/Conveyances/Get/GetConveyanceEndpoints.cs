using CodePunk.Conveyancing.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Features.Conveyances.Get;

public static class GetConveyanceEndpoints
{
    public static IEndpointRouteBuilder MapGetConveyanceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/conveyances");

        group.MapGet("/{id:guid}", async (Guid id, ConveyancingDbContext db, CancellationToken ct) =>
        {
            var conveyance = await db.Conveyances.FirstOrDefaultAsync(c => c.Id == id, ct);
            return conveyance is not null ? Results.Ok(conveyance) : Results.NotFound();
        });

        return routes;
    }
}
