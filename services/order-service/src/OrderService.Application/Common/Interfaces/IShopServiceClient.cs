namespace OrderService.Application.Common.Interfaces;

public interface IShopServiceClient
{
    Task<ShopPickupAddressResult> GetPickupAddressAsync(Guid shopId, CancellationToken cancellationToken);
}

public record ShopPickupAddressResult(bool IsValid, AddressSnapshot? PickupAddressSnapshot, string? FailureReason);
