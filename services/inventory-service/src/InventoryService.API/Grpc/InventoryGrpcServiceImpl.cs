using Common.Contracts.Grpc.Inventory;
using Grpc.Core;
using InventoryService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.API.Grpc;

public class InventoryGrpcServiceImpl(IApplicationDbContext dbContext)
    : InventoryGrpcService.InventoryGrpcServiceBase
{
    public override async Task<GetCombinationPricesResponse> GetCombinationPrices(
        GetCombinationPricesRequest request, ServerCallContext context)
    {
        List<Guid> ids = [.. request.CombinationIds.Select(Guid.Parse)];

        var combinations = await dbContext.ProductVariantCombinations
            .AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.ShopId, c.Price })
            .ToListAsync(context.CancellationToken);

        GetCombinationPricesResponse response = new();
        response.Items.AddRange(combinations.Select(c => new CombinationPrice
        {
            CombinationId = c.Id.ToString(), ShopId = c.ShopId.ToString(), Price = c.Price
        }));

        return response;
    }
}
