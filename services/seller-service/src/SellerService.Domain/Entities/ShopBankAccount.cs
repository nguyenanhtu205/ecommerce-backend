namespace SellerService.Domain.Entities;

public class ShopBankAccount : BaseEntity
{
    public required Guid ShopId { get; init; }

    public required string BankName { get; init; }

    public required string AccountNumber { get; init; }

    public required string AccountHolder { get; init; }

    public required bool IsDefault { get; init; }

    public required bool IsVerified { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public Shop? Shop { get; init; }
}
