namespace SellerService.Application.Features.Commands.UpdateShopBasicInformation;

public record UpdateShopBasicInformationCommand(string? Name, string? Description, string? ShopAvatarUrl) : IRequest;

public class UpdateShopBasicInformation(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    ITopicProducer<ShopNameChanged> producer)
    : IRequestHandler<UpdateShopBasicInformationCommand>
{
    public async Task Handle(UpdateShopBasicInformationCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Shop? shop = await context.Shops
            .Where(x => x.OwnerUserId == currentUser.UserId.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (shop is null)
        {
            throw new NotFoundException("Shop not found");
        }

        shop.Name = request.Name ?? shop.Name;
        shop.Description = request.Description ?? shop.Description;
        shop.ShopAvatarUrl = request.ShopAvatarUrl ?? shop.ShopAvatarUrl;

        await context.SaveChangesAsync(cancellationToken);

        if (request.Name != null)
        {
            await producer.Produce(new ShopNameChanged(shop.Id.ToString(), request.Name, DateTimeOffset.UtcNow),
                cancellationToken);
        }
    }
}
