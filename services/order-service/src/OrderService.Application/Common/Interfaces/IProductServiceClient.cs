namespace OrderService.Application.Common.Interfaces;

public interface IProductServiceClient
{
    Task<IReadOnlyDictionary<Guid, ProductInfo>> GetProductInfosAsync(IEnumerable<Guid> productIds,
        CancellationToken cancellationToken);
}

public record ProductInfo(
    string ProductName,
    string ThumbnailUrl,
    int WeightGram,
    int Length,
    int Width,
    int Height);
