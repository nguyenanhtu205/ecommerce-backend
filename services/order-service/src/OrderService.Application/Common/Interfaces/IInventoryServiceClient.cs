namespace OrderService.Application.Common.Interfaces;

public interface IInventoryServiceClient
{
    Task<IReadOnlyDictionary<Guid, CombinationPriceInfo>> GetPricesAsync(IEnumerable<Guid> combinationIds,
        CancellationToken cancellationToken);
}

public record CombinationPriceInfo(Guid CombinationId, Guid ShopId, Guid ProductId, int Price);
