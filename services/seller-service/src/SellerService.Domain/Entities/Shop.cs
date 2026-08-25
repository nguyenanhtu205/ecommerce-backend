namespace SellerService.Domain.Entities;

public class Shop : BaseEntity
{
    public required Guid OwnerUserId { get; init; }

    public required string Name { get; set; }

    public required string Email { get; init; }

    public string? Description { get; set; }

    public string? ShopAvatarUrl { get; set; }

    public Guid? PickupAddressId { get; init; }

    public required AddressSnapshot PickupAddressSnapshot { get; set; }

    public required ShopStatus Status { get; set; } = ShopStatus.PendingSetup;

    public required bool IsLinkedToMainAccount { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public ShopPaymentSetting? ShopPaymentSetting { get; init; }

    public ShopVacationSetting? ShopVacationSetting { get; init; }

    public ShopChatSetting? ShopChatSetting { get; init; }

    public ICollection<ShopChatQuickReply> ShopChatQuickReplies { get; private set; } = new List<ShopChatQuickReply>();

    public ICollection<ShopShippingCarrierConnection> ShippingCarrierConnections { get; private set; } =
        new List<ShopShippingCarrierConnection>();

    public ICollection<ShopBankAccount> ShopBankAccounts { get; private set; } = new List<ShopBankAccount>();
}
