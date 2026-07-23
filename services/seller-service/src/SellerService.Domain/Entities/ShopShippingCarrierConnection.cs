namespace SellerService.Domain.Entities;

public class ShopShippingCarrierConnection : BaseEntity
{
    public required Guid ShopId { get; init; }

    public required Guid CarrierId { get; init; }

    public required ShopShippingCarrierConnectionStatus Status { get; init; }

    public DateTimeOffset? ConnectedAt { get; init; }

    public Shop? Shop { get; init; }
}
