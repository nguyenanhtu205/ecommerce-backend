namespace SellerService.Application.Features.Queries.GetShopInformation;

public record GetShopShippingInfoItem(string Id, string Province, string Ward, List<string> CarrierCode);

public record GetShopShippingInfoQuery(List<string> ShopIds) : IRequest<List<GetShopShippingInfoItem>>;

public class GetShopShippingInfo(IApplicationDbContext context)
    : IRequestHandler<GetShopShippingInfoQuery, List<GetShopShippingInfoItem>>
{
    public async Task<List<GetShopShippingInfoItem>> Handle(GetShopShippingInfoQuery request,
        CancellationToken cancellationToken)
    {
        List<Guid> shopIds =
        [
            .. request.ShopIds
                .Select(id => Guid.TryParse(id, out Guid guid) ? guid : (Guid?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
        ];

        List<GetShopShippingInfoItem> shops = await context.Shops
            .AsNoTracking()
            .Where(x => shopIds.Contains(x.Id))
            .Include(x => x.ShippingCarrierConnections)
            .Select(x => new GetShopShippingInfoItem(
                x.Id.ToString(),
                x.PickupAddressSnapshot.Province,
                x.PickupAddressSnapshot.Ward,
                x.ShippingCarrierConnections.Select(s => s.CarrierCode).ToList()))
            .ToListAsync(cancellationToken);

        Dictionary<string, GetShopShippingInfoItem> shopDict = shops.ToDictionary(x => x.Id);

        return
        [
            .. request.ShopIds
                .Where(shopDict.ContainsKey)
                .Select(id => shopDict[id])
        ];
    }
}
