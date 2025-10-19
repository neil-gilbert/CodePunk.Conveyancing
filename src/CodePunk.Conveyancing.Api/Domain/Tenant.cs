namespace CodePunk.Conveyancing.Api.Domain;

public sealed class Tenant
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? BrandJson { get; set; }
    public string? SsoConfigJson { get; set; }
    public string BillingPlan { get; set; } = "mvp";
    public bool Active { get; set; } = true;
    public DateTime CreatedUtc { get; set; }
}

