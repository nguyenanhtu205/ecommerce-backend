using SellerService.Domain.Common;

namespace SellerService.Application.Features.Queries.GetShopInformation;

public record GetShopInformationResponse(string Email, AddressSnapshot PickupAddressSnapshot);

public record GetShopInformationQuery : IRequest<GetShopInformationResponse>;

public class GetShopInformationQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetShopInformationQuery, GetShopInformationResponse>
{
    public async Task<GetShopInformationResponse> Handle(GetShopInformationQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new ForbiddenAccessException();
        }

        Guid sellerId = currentUser.UserId.Value;

        Shop? shop = await context.Shops
            .AsNoTracking()
            .Where(s => s.OwnerUserId == sellerId)
            .FirstOrDefaultAsync(cancellationToken);


        return shop == null
            ? throw new NotFoundException("Shop not found")
            : new GetShopInformationResponse(shop.Email, shop.PickupAddressSnapshot);
        ;
    }
}
