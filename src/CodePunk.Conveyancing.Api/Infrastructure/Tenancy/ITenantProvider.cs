namespace CodePunk.Conveyancing.Api.Infrastructure.Tenancy;

public interface ITenantProvider
{
    Guid? TenantId { get; set; }
    string? TenantSlug { get; set; }
}

internal sealed class TenantProvider : ITenantProvider
{
    public Guid? TenantId { get; set; }
    public string? TenantSlug { get; set; }
}

