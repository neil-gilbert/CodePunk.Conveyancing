namespace CodePunk.Conveyancing.Api.Domain;

public sealed class Conveyance
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public string PropertyAddress { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
}
