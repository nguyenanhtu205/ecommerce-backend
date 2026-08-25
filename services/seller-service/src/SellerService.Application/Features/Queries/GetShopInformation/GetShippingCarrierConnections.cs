namespace SellerService.Application.Features.Queries.GetShopInformation;

public record CarrierItem(string CarrierCode, ShopShippingCarrierConnectionStatus Status);

public record GetShippingCarrierConnectionsQuery : IRequest<List<CarrierItem>>;

public class GetShippingCarrierConnections(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetShippingCarrierConnectionsQuery, List<CarrierItem>>
{
    public async Task<List<CarrierItem>> Handle(GetShippingCarrierConnectionsQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        List<ShopShippingCarrierConnection> connections = await context.ShopShippingCarrierConnections
            .AsNoTracking()
            .Where(c => c.ShopId == currentUser.ShopId.Value)
            .ToListAsync(cancellationToken);

        return [.. connections.Select(c => new CarrierItem(c.CarrierCode, c.Status))];
    }
}
