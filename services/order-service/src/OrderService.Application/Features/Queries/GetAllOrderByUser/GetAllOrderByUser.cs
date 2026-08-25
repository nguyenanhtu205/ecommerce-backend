namespace OrderService.Application.Features.Queries.GetAllOrderByUser;

public record GetAllOrderByUserResponse(
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
    List<OrderItemDto> Items);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Thumbnail,
    string? Variation,
    int Quantity,
    int Price,
    int? OriginalPrice);

public record GetAllOrderByUserQuery : IRequest<List<GetAllOrderByUserResponse>>;

public class GetAllOrderByUser(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetAllOrderByUserQuery, List<GetAllOrderByUserResponse>>
{
    public async Task<List<GetAllOrderByUserResponse>> Handle(
        GetAllOrderByUserQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Guid buyerId = currentUser.UserId.Value;

        List<Order> orders = await context.Orders
            .AsNoTracking()
            .Where(o => o.BuyerId == buyerId)
            .Include(o => o.OrderItems)
            .ToListAsync(cancellationToken);

        return
        [
            .. orders.Select(o => new GetAllOrderByUserResponse(o.Id, o.ShopId, o.ShopName, o.Status.ToString(),
                o.MerchandiseSubtotal, o.ShippingFee, o.VoucherDiscount, o.XuDiscount, o.TotalPayment, o.Note,
                o.CreatedAt, o.UpdatedAt,
                [
                    .. o.OrderItems.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductName, i.ThumbnailUrl,
                        i.Variation, i.Quantity, i.Price, i.OriginalPrice))
                ]))
        ];
    }
}
