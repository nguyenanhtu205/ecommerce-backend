namespace PaymentService.Domain.Entities;

public class Refund : BaseEntity
{
    public required Guid PaymentId { get; init; }

    public required Guid OrderId { get; init; }

    public required int Amount { get; init; }

    public string? Reason { get; init; }

    public required RefundStatus Status { get; init; }

    public required string IdempotencyKey { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public Payment? Payment { get; init; }

    public ICollection<ShopWalletTransaction> ShopWalletTransactions { get; private set; } =
        new List<ShopWalletTransaction>();
}
