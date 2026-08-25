namespace SellerService.Application.Features.Queries.GetShopInformation;

public record GetShopBasicInformationResponse(string? Name, string? Description, string? ShopAvatarUrl);

public record GetShopBasicInformationQuery : IRequest<GetShopBasicInformationResponse>;

public class GetShopBasicInformation(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetShopBasicInformationQuery, GetShopBasicInformationResponse>
{
    public async Task<GetShopBasicInformationResponse> Handle(GetShopBasicInformationQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Shop? shop = await context.Shops
            .AsNoTracking()
            .Where(x => x.OwnerUserId == currentUser.UserId.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return shop is null
            ? throw new NotFoundException("Shop not found")
            : new GetShopBasicInformationResponse(shop.Name, shop.Description, shop.ShopAvatarUrl);
    }
}
