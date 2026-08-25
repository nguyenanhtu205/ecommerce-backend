namespace OrderService.Application.Features.Queries.GetOrderItemInfo;

public record OrderItemInfo(Guid Id, Guid OrderId, string ProductName, string ThumbnailUrl, int Quantity);

public record GetOrderItemInfoQuery(List<Guid> OrderItemIds) : IRequest<List<OrderItemInfo>>;

public class GetOrderItemInfo(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetOrderItemInfoQuery, List<OrderItemInfo>>
{
    public async Task<List<OrderItemInfo>> Handle(GetOrderItemInfoQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        List<OrderItem> items = await context.OrderItems
            .AsNoTracking()
            .Where(i => request.OrderItemIds.Contains(i.Id))
            .ToListAsync(cancellationToken);

        return [.. items.Select(i => new OrderItemInfo(i.Id, i.OrderId, i.ProductName, i.ThumbnailUrl, i.Quantity))];
    }
}
