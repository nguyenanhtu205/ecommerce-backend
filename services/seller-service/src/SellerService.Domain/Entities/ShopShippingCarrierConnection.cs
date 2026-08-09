namespace SellerService.Domain.Entities;

public class ShopShippingCarrierConnection : BaseEntity
{
    public required Guid ShopId { get; init; }

    public required Guid CarrierId { get; init; }

    public required ShopShippingCarrierConnectionStatus Status { get; set; }

    public DateTimeOffset? ConnectedAt { get; set; }

    public Shop? Shop { get; init; }
}
