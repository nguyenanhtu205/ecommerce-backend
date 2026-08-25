using Common.Contracts.Grpc.Inventory;

namespace OrderService.Infrastructure.Services;

public class InventoryServiceClient(
    InventoryGrpcService.InventoryGrpcServiceClient grpcClient) : IInventoryServiceClient
{
    public async Task<IReadOnlyDictionary<Guid, CombinationPriceInfo>> GetPricesAsync(
        IEnumerable<Guid> combinationIds, CancellationToken cancellationToken)
    {
        GetCombinationPricesRequest request = new();
        request.CombinationIds.AddRange(combinationIds.Select(id => id.ToString()));

        GetCombinationPricesResponse response = await grpcClient.GetCombinationPricesAsync(
            request, deadline: DateTime.UtcNow.AddSeconds(3), cancellationToken: cancellationToken);

        return response.Items.ToDictionary(
            item => Guid.Parse(item.CombinationId),
            item => new CombinationPriceInfo(
                Guid.Parse(item.CombinationId),
                Guid.Parse(item.ShopId),
                Guid.Parse(item.ProductId),
                item.Price));
    }
}
