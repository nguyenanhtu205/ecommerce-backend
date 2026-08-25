namespace PaymentService.Domain.Entities;

public class ShopWallet
{
    public required Guid ShopId { get; init; }

    public required int AvailableBalance { get; set; }

    public required int PendingBalance { get; set; }

    public required int DebtBalance { get; set; }

    public required DateTimeOffset UpdatedAt { get; set; }

    public ICollection<ShopWalletTransaction> ShopWalletTransactions { get; private set; } =
        new List<ShopWalletTransaction>();
}
