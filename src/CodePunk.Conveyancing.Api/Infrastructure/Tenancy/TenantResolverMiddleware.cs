using System.Text.RegularExpressions;
using CodePunk.Conveyancing.Api.Data;
using CodePunk.Conveyancing.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CodePunk.Conveyancing.Api.Infrastructure.Tenancy;

public sealed class TenantResolverMiddleware(RequestDelegate next)
{
    private static readonly Regex SubdomainRegex = new(
        pattern: "^(?<sub>[^.]+)\\.",
        options: RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task InvokeAsync(HttpContext context, ITenantProvider provider, ConveyancingDbContext db)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Only enforce tenant resolution for API routes
        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // Skip resolution for admin provisioning endpoints
        if (path.StartsWith("/api/admin/tenants", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        string? slug = null;

        // 1) X-Tenant header (dev/testing convenience)
        if (context.Request.Headers.TryGetValue("X-Tenant", out var header) && !string.IsNullOrWhiteSpace(header))
        {
            slug = header.ToString().Trim();
        }

        // 2) Subdomain extraction if not localhost
        if (slug is null)
        {
            var host = context.Request.Host.Host;
            var isLocal = string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase);
            if (!isLocal)
            {
                var m = SubdomainRegex.Match(host);
                if (m.Success)
                {
                    slug = m.Groups["sub"].Value.ToLowerInvariant();
                    if (slug is "www") slug = null;
                }
            }
        }

        // 3) Path fallback: /t/{slug}/...
        if (slug is null && path.StartsWith("/t/", StringComparison.OrdinalIgnoreCase))
        {
            var segs = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segs.Length >= 2)
            {
                slug = segs[1].ToLowerInvariant();
            }
        }

        if (slug is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant not resolved. Provide X-Tenant or use tenant subdomain/path." });
            return;
        }

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug);
        if (tenant is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant not found", slug });
            return;
        }

        provider.TenantId = tenant.Id;
        provider.TenantSlug = tenant.Slug;

        await next(context);
    }
}
