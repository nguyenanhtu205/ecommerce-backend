using System.Globalization;
using Common.Contracts.Grpc.Shop;
using AddressSnapshot = OrderService.Domain.Common.AddressSnapshot;
using PickupAddressSnapshot = Common.Contracts.Grpc.Shop.AddressSnapshot;

namespace OrderService.Infrastructure.Services;

public class ShopServiceClient(
    ShopGrpcService.ShopGrpcServiceClient grpcClient) : IShopServiceClient
{
    public async Task<ShopPickupAddressResult> GetPickupAddressAsync(
        Guid shopId, CancellationToken cancellationToken)
    {
        GetPickupAddressRequest request = new() { ShopId = shopId.ToString() };

        GetPickupAddressResponse response = await grpcClient.GetPickupAddressAsync(
            request, deadline: DateTime.UtcNow.AddSeconds(3), cancellationToken: cancellationToken);

        if (!response.IsValid || response.PickupAddressSnapshot is null)
        {
            return new ShopPickupAddressResult(false, response.ShopName, null, response.FailureReason);
        }

        PickupAddressSnapshot p = response.PickupAddressSnapshot;
        AddressSnapshot snapshot = new()
        {
            UserId = Guid.Parse(p.UserId),
            FullName = p.FullName,
            Phone = p.Phone,
            Province = p.Province,
            Ward = p.Ward,
            AddressDetail = p.AddressDetail,
            FullAddressText = p.FullAddressText,
            Latitude = p.HasLatitude ? decimal.Parse(p.Latitude, CultureInfo.InvariantCulture) : null,
            Longitude = p.HasLongitude ? decimal.Parse(p.Longitude, CultureInfo.InvariantCulture) : null,
            AddressType = p.AddressType
        };

        return new ShopPickupAddressResult(true, response.ShopName, snapshot, null);
    }
}
