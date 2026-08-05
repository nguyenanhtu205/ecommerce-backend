namespace PaymentService.Domain.Entities;

public class ShopWalletTransaction : BaseEntity
{
    public required Guid ShopId { get; init; }

    public Guid? OrderId { get; init; }

    public Guid? EscrowHoldId { get; init; }

    public Guid? RefundId { get; init; }

    public required WalletTransactionType Type { get; init; }

    public required int Amount { get; init; }

    public required int AvailableBalanceAfter { get; init; }

    public required int DebtBalanceAfter { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public ShopWallet? ShopWallet { get; init; }

    public EscrowHold? EscrowHold { get; init; }

    public Refund? Refund { get; init; }
}
