using CodePunk.Conveyancing.Api.Domain;

namespace CodePunk.Conveyancing.Api.Infrastructure;

public static class InMemoryDraftStore
{
    private static readonly Dictionary<Guid, DraftDocument> Drafts = new();
    private static readonly Dictionary<Guid, List<Guid>> DraftsByConveyance = new();

    public static DraftDocument AddDraft(Guid conveyanceId, DraftType type, string contentMarkdown, Dictionary<string, string>? metadata = null)
    {
        var draft = new DraftDocument
        {
            Id = Guid.NewGuid(),
            ConveyanceId = conveyanceId,
            Type = type,
            Status = DraftStatus.Draft,
            ContentMarkdown = contentMarkdown,
            CreatedUtc = DateTime.UtcNow,
        };

        if (metadata is not null)
        {
            foreach (var kv in metadata)
            {
                draft.Metadata[kv.Key] = kv.Value;
            }
        }

        Drafts[draft.Id] = draft;
        if (!DraftsByConveyance.TryGetValue(conveyanceId, out var list))
        {
            list = new List<Guid>();
            DraftsByConveyance[conveyanceId] = list;
        }
        list.Add(draft.Id);
        return draft;
    }

    public static bool TryGetDraft(Guid id, out DraftDocument? draft)
        => Drafts.TryGetValue(id, out draft);

    public static IReadOnlyList<DraftDocument> GetDraftsForConveyance(Guid conveyanceId)
    {
        if (DraftsByConveyance.TryGetValue(conveyanceId, out var ids))
        {
            return ids.Select(id => Drafts[id]).ToList();
        }
        return Array.Empty<DraftDocument>();
    }

    public static bool ApproveDraft(Guid id, string approvedBy)
    {
        if (!Drafts.TryGetValue(id, out var draft)) return false;
        draft.Status = DraftStatus.Approved;
        draft.ApprovedBy = approvedBy;
        draft.ApprovedUtc = DateTime.UtcNow;
        return true;
    }

    public static bool RejectDraft(Guid id, string rejectedBy, string? reason)
    {
        if (!Drafts.TryGetValue(id, out var draft)) return false;
        draft.Status = DraftStatus.Rejected;
        draft.RejectedBy = rejectedBy;
        draft.RejectedUtc = DateTime.UtcNow;
        draft.RejectionReason = reason;
        return true;
    }
}

