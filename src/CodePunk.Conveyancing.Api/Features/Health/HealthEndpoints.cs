using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodePunk.Conveyancing.Api.Features.Health;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/health");
        group.MapGet("/", () => Results.Ok(new { status = "ok" }));
        return routes;
    }
}

