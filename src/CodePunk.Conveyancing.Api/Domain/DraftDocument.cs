namespace CodePunk.Conveyancing.Api.Domain;

public enum DraftStatus
{
    Draft = 0,
    Approved = 1,
    Rejected = 2
}

public enum DraftType
{
    TitleEnquiries = 1
}

public sealed class DraftDocument
{
    public Guid TenantId { get; set; }
    public Guid Id { get; init; }
    public Guid ConveyanceId { get; init; }
    public DraftType Type { get; init; }
    public DraftStatus Status { get; set; }
    public string ContentMarkdown { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; init; }
    public string CreatedBy { get; init; } = "agent";
    public DateTime? ApprovedUtc { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? RejectedUtc { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }
    public Dictionary<string, string> Metadata { get; init; } = new();
}
