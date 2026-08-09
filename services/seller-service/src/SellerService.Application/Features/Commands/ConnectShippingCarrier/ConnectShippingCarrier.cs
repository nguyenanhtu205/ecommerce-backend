namespace SellerService.Application.Features.Commands.ConnectShippingCarrier;

public record ConnectShippingCarrierResult(Guid ConnectionId, ShopShippingCarrierConnectionStatus Status);

public record ConnectShippingCarrierCommand(Guid ShopId, Guid CarrierId) : IRequest<ConnectShippingCarrierResult>;

public class ConnectShippingCarrierCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<ConnectShippingCarrierCommand, ConnectShippingCarrierResult>
{
    public async Task<ConnectShippingCarrierResult> Handle(ConnectShippingCarrierCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        Guid sellerId = currentUser.UserId.Value;

        Shop? shop = await context.Shops.FirstOrDefaultAsync(s => s.Id == request.ShopId, cancellationToken);

        if (shop == null)
        {
            throw new NotFoundException("Shop  does not exist.");
        }

        if (shop.OwnerUserId != sellerId)
        {
            throw new ForbiddenAccessException();
        }

        ShopShippingCarrierConnection? connection = await context.ShopShippingCarrierConnections
            .FirstOrDefaultAsync(
                c => c.ShopId == request.ShopId && c.CarrierId == request.CarrierId, cancellationToken);

        if (connection == null)
        {
            connection = new ShopShippingCarrierConnection
            {
                ShopId = request.ShopId,
                CarrierId = request.CarrierId,
                Status = ShopShippingCarrierConnectionStatus.Connected,
                ConnectedAt = DateTimeOffset.UtcNow
            };
            context.ShopShippingCarrierConnections.Add(connection);
        }
        else
        {
            connection.Status = ShopShippingCarrierConnectionStatus.Connected;
            connection.ConnectedAt = DateTimeOffset.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new ConnectShippingCarrierResult(connection.Id, connection.Status);
    }
}
