namespace InventoryService.Application.Features.Queries.GetInventoryForSeller;

public record GetInventoryForSellerResponse(
    Guid Id,
    Guid ProductId,
    int Price,
    int Stock,
    int ReservedStock);

public record GetInventoryForSellerQuery : IRequest<List<GetInventoryForSellerResponse>>;

public class GetInventoryForSeller(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetInventoryForSellerQuery, List<GetInventoryForSellerResponse>>
{
    public async Task<List<GetInventoryForSellerResponse>> Handle(GetInventoryForSellerQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Guid shopId = currentUser.ShopId.Value;

        List<ProductVariantCombination> inventories = await context.ProductVariantCombinations
            .AsNoTracking()
            .Where(x => x.ShopId == shopId)
            .ToListAsync(cancellationToken);

        return
        [
            .. inventories.Select(i =>
                new GetInventoryForSellerResponse(i.Id, i.ProductId, i.Price, i.Stock, i.ReservedStock))
        ];
    }
}
