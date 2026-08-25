namespace PaymentService.Domain.Entities;

public class EscrowHold : BaseEntity
{
    public required Guid OrderId { get; init; }

    public required Guid PaymentId { get; init; }

    public required Guid ShopId { get; init; }

    public required int Amount { get; init; }

    public required EscrowStatus Status { get; set; }

    public required DateTimeOffset HeldAt { get; init; }

    public required DateTimeOffset ReleaseDueAt { get; init; }

    public DateTimeOffset? ReleasedAt { get; set; }

    public Payment? Payment { get; init; }

    public ICollection<ShopWalletTransaction> ShopWalletTransactions { get; private set; } =
        new List<ShopWalletTransaction>();
}
