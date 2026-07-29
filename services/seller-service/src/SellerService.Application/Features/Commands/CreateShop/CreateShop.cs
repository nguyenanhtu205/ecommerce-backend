namespace SellerService.Application.Features.Commands.CreateShop;

public record CreateShopResponse(Guid ShopId);

public record CreateShopCommand(string Name, string Email, Guid PickupAddressId) : IRequest<CreateShopResponse>;

public class CreateShop(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<CreateShopCommand, CreateShopResponse>
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
            Status = ShopStatus.PendingSetup,
            IsLinkedToMainAccount = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Shops.Add(shop);

        await context.SaveChangesAsync(cancellationToken);

        return new CreateShopResponse(shop.Id);
    }
}
