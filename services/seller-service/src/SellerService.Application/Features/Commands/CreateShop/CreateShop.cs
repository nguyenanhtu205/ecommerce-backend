using SellerService.Domain.Common;

namespace SellerService.Application.Features.Commands.CreateShop;

public record CreateShopResponse(Guid ShopId);

public record CreateShopCommand(string Name, string Email, Guid PickupAddressId, AddressSnapshot PickupAddressSnapshot)
    : IRequest<CreateShopResponse>;

public class CreateShop(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    ITopicProducer<ShopCreated> producer) : IRequestHandler<CreateShopCommand, CreateShopResponse>
{
    public async Task<CreateShopResponse> Handle(CreateShopCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Guid userId = currentUser.UserId.Value;

        Shop shop = new()
        {
            OwnerUserId = userId,
            Name = request.Name,
            Email = request.Email,
            PickupAddressId = request.PickupAddressId,
            PickupAddressSnapshot = request.PickupAddressSnapshot,
            Status = ShopStatus.PendingSetup,
            IsLinkedToMainAccount = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Shops.Add(shop);

        await producer.Produce(new ShopCreated(userId, shop.Id, shop.Name, DateTimeOffset.UtcNow), cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new CreateShopResponse(shop.Id);
    }
}
