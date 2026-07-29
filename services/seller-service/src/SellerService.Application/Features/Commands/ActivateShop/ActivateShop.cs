namespace SellerService.Application.Features.Commands.ActivateShop;

public record ActivateShopCommand(Guid ShopId, string Email) : IRequest;

public class ActivateShop(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    ITopicProducer<ShopActivated> producer) : IRequestHandler<ActivateShopCommand>
{
    public async Task Handle(ActivateShopCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Guid userId = currentUser.UserId.Value;

        Shop? shop = await context.Shops.FindAsync([request.ShopId], cancellationToken);

        if (shop is null)
        {
            throw new NotFoundException("Shop not found");
        }

        if (shop.OwnerUserId != userId)
        {
            throw new ForbiddenAccessException();
        }

        shop.Status = ShopStatus.Active;

        await producer.Produce(new ShopActivated(shop.Email, "activate-shop", DateTimeOffset.UtcNow),
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
