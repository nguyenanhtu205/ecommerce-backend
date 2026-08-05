namespace OrderService.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public required Guid OrderId { get; init; }

    public required OrderStatus Status { get; init; }

    public required DateTimeOffset ChangedAt { get; init; }

    public string? ChangedBy { get; init; }

    public Order? Order { get; init; }
}
