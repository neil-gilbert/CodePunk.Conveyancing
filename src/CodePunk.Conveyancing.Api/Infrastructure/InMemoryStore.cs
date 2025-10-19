using CodePunk.Conveyancing.Api.Domain;

namespace CodePunk.Conveyancing.Api.Infrastructure;

public static class InMemoryStore
{
    private static readonly Dictionary<Guid, Conveyance> Conveyances = new();

    public static Conveyance AddConveyance(string buyerName, string sellerName, string propertyAddress)
    {
        var item = new Conveyance
        {
            Id = Guid.NewGuid(),
            BuyerName = buyerName,
            SellerName = sellerName,
            PropertyAddress = propertyAddress,
            CreatedUtc = DateTime.UtcNow
        };

        Conveyances[item.Id] = item;
        return item;
    }

    public static bool TryGetConveyance(Guid id, out Conveyance? conveyance)
    {
        return Conveyances.TryGetValue(id, out conveyance);
    }
}

