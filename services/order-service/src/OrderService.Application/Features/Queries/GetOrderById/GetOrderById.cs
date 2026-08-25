namespace OrderService.Application.Features.Queries.GetOrderById;

public record GetOrderByIdResponse(
    Guid Id,
    Guid ShopId,
    string ShopName,
    string Status,
    int MerchandiseSubtotal,
    int ShippingFee,
    int TotalDiscount,
    int XuDiscount,
    int TotalPayment,
    string? Note,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    List<OrderItem> Items);

public record OrderItem(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Thumbnail,
    string? Variation,
    int Quantity,
    int Price,
    int? OriginalPrice);

public record GetOrderByIdQuery(Guid OrderId) : IRequest<GetOrderByIdResponse>;

public class GetOrderById(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetOrderByIdQuery, GetOrderByIdResponse>
{
    public async Task<GetOrderByIdResponse> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Order? order = await context.Orders
            .AsNoTracking()
            .Where(o => o.Id == request.OrderId)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order not found");
        }

        if (order.BuyerId != currentUser.UserId.Value)
        {
            throw new ForbiddenAccessException();
        }

        return new GetOrderByIdResponse(order.Id, order.ShopId, order.ShopName, order.Status.ToString(),
            order.MerchandiseSubtotal, order.ShippingFee, order.VoucherDiscount, order.XuDiscount, order.TotalPayment,
            order.Note,
            order.CreatedAt, order.UpdatedAt,
            [
                .. order.OrderItems.Select(i => new OrderItem(i.Id, i.ProductId, i.ProductName, i.ThumbnailUrl,
                    i.Variation, i.Quantity, i.Price, i.OriginalPrice))
            ]);
    }
}
