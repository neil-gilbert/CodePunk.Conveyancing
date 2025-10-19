namespace CodePunk.Conveyancing.Api.Domain;

public sealed class Contact
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedUtc { get; set; }
}

public enum ConveyanceContactRole
{
    Buyer = 1,
    Seller = 2,
    FeeEarner = 3,
    Lender = 4,
    Other = 99
}

public sealed class ConveyanceContact
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; }
    public Guid ConveyanceId { get; set; }
    public Guid ContactId { get; set; }
    public ConveyanceContactRole Role { get; set; }
    public bool IsClientOfTenant { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedUtc { get; set; }
}

