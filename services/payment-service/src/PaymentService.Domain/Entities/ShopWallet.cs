namespace PaymentService.Domain.Entities;

public class ShopWallet
{
    public required Guid ShopId { get; init; }

    public required int AvailableBalance { get; init; }

    public required int PendingBalance { get; init; }

    public required int DebtBalance { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public ICollection<ShopWalletTransaction> ShopWalletTransactions { get; private set; } =
        new List<ShopWalletTransaction>();
}
