namespace OrderService.Domain.Entities;

public class Order : BaseEntity
{
    public required Guid BuyerId { get; init; }

    public required Guid ShopId { get; init; }

    public required Guid CheckoutBatchId { get; init; }

    public required OrderStatus Status { get; set; }

    public required int MerchandiseSubtotal { get; init; }

    public required int ShippingFee { get; init; }

    public required int VoucherDiscount { get; init; }

    public required int XuDiscount { get; init; }

    public required int TotalPayment { get; init; }

    public required AddressSnapshot ShippingAddressSnapshot { get; init; }

    public string? Note { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; set; }

    public OrderShippingSnapshot? OrderShippingSnapshot { get; init; }

    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

    public ICollection<OrderVoucher> OrderVouchers { get; private set; } = new List<OrderVoucher>();

    public ICollection<OrderStatusHistory> OrderStatusHistories { get; private set; } = new List<OrderStatusHistory>();
}
