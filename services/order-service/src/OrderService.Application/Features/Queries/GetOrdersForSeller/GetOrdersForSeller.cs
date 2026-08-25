namespace OrderService.Application.Features.Queries.GetOrdersForSeller;

public record GetOrderForSellerResponse(
    Guid Id,
    string Status,
    int MerchandiseSubtotal,
    int ShippingFee,
    int TotalDiscount,
    int XuDiscount,
    int TotalPayment,
    string? Note,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    List<SellerOrderItem> Items);

public record SellerOrderItem(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Thumbnail,
    string? Variation,
    int Quantity,
    int Price,
    int? OriginalPrice);

public record GetOrdersForSellerQuery : IRequest<List<GetOrderForSellerResponse>>;

public class GetOrdersForSeller(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetOrdersForSellerQuery, List<GetOrderForSellerResponse>>
{
    public async Task<List<GetOrderForSellerResponse>> Handle(GetOrdersForSellerQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Guid shopId = currentUser.ShopId.Value;

        List<Order> orders = await context.Orders
            .AsNoTracking()
            .Where(o => o.ShopId == shopId)
            .Include(o => o.OrderItems)
            .ToListAsync(cancellationToken);

        return
        [
            .. orders.Select(o => new GetOrderForSellerResponse(
                o.Id, o.Status.ToString(), o.MerchandiseSubtotal, o.ShippingFee, o.VoucherDiscount, o.XuDiscount,
                o.TotalPayment, o.Note, o.CreatedAt, o.UpdatedAt,
                [
                    .. o.OrderItems.Select(i => new SellerOrderItem(i.Id, i.ProductId, i.ProductName,
                        i.ThumbnailUrl, i.Variation, i.Quantity, i.Price, i.OriginalPrice))
                ]))
        ];
    }
}
