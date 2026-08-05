namespace OrderService.Domain.Entities;

public class OrderVoucher : BaseEntity
{
    public required Guid OrderId { get; init; }

    public string? VoucherCode { get; init; }

    public required int DiscountAmount { get; init; }

    public required string Scope { get; init; }

    public Order? Order { get; init; }
}
