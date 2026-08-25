namespace SellerService.Application.Features.Queries.GetShopInformation;

public record GetShopsInfoForChatItem(string Id, string Name, string? ShopAvatarUrl);

public record GetShopsInfoForChatQuery(List<string> ShopIds) : IRequest<List<GetShopsInfoForChatItem>>;

public class GetShopsInfoForChat(IApplicationDbContext context)
    : IRequestHandler<GetShopsInfoForChatQuery, List<GetShopsInfoForChatItem>>
{
    public async Task<List<GetShopsInfoForChatItem>> Handle(GetShopsInfoForChatQuery request,
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

        List<GetShopsInfoForChatItem> shops = await context.Shops
            .AsNoTracking()
            .Where(x => shopIds.Contains(x.Id))
            .Select(x => new GetShopsInfoForChatItem(x.Id.ToString(), x.Name, x.ShopAvatarUrl))
            .ToListAsync(cancellationToken);

        Dictionary<string, GetShopsInfoForChatItem> shopDict = shops.ToDictionary(x => x.Id);

        return
        [
            .. request.ShopIds
                .Where(shopDict.ContainsKey)
                .Select(id => shopDict[id])
        ];
    }
}
