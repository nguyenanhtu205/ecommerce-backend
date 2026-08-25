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

        string carrierCode = request.CarrierId == Guid.Parse("11111111-1111-1111-1111-111111111111")
            ? "mock"
            : request.CarrierId == Guid.Parse("22222222-2222-2222-2222-222222222222")
                ? "ghn"
                : request.CarrierId == Guid.Parse("33333333-3333-3333-3333-333333333333")
                    ? "ghtk"
                    : "unknown";

        if (connection == null)
        {
            connection = new ShopShippingCarrierConnection
            {
                ShopId = request.ShopId,
                CarrierId = request.CarrierId,
                CarrierCode = carrierCode,
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
