using Common.Contracts.Grpc.Product;

namespace OrderService.Infrastructure.Services;

public class ProductServiceClient(
    ProductGrpcService.ProductGrpcServiceClient grpcClient) : IProductServiceClient
{
    public async Task<IReadOnlyDictionary<Guid, ProductInfo>> GetProductInfosAsync(
        IEnumerable<Guid> productIds, CancellationToken cancellationToken)
    {
        GetProductInfosRequest request = new();
        request.ProductIds.AddRange(productIds.Select(id => id.ToString()));

        GetProductInfosResponse response = await grpcClient.GetProductInfosAsync(
            request, deadline: DateTime.UtcNow.AddSeconds(3), cancellationToken: cancellationToken);

        return response.Items.ToDictionary(
            item => Guid.Parse(item.ProductId),
            item => new ProductInfo(
                item.ProductName,
                item.ThumbnailUrl,
                item.WeightGram,
                item.Length,
                item.Width,
                item.Height));
    }
}
