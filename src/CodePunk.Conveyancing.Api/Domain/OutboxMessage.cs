namespace CodePunk.Conveyancing.Api.Domain;

public enum OutboxStatus
{
    DraftedByAgent = 0,
    EditedByFeeEarner = 1,
    Approved = 2,
    Sent = 3,
    Failed = 4
}

public sealed class OutboxMessage
{
    public Guid TenantId { get; set; }
    public Guid Id { get; set; }
    public Guid ConveyanceId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyMarkdown { get; set; } = string.Empty;
    public List<string> ToRecipients { get; set; } = new();
    public OutboxStatus Status { get; set; } = OutboxStatus.DraftedByAgent;
    public DateTime CreatedUtc { get; set; }
    public string CreatedBy { get; set; } = "agent";
    public DateTime? ApprovedUtc { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? SentUtc { get; set; }
    public string? FailureReason { get; set; }
    public Guid? SourceDraftId { get; set; }
}
