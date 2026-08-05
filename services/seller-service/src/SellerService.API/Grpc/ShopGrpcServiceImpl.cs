using System.Globalization;
using Common.Contracts.Grpc.Shop;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using SellerService.Application.Common.Interfaces;
using AddressSnapshot = SellerService.Domain.Common.AddressSnapshot;

namespace SellerService.API.Grpc;

public class ShopGrpcServiceImpl(IApplicationDbContext dbContext) : ShopGrpcService.ShopGrpcServiceBase
{
    public override async Task<GetPickupAddressResponse> GetPickupAddress(
        GetPickupAddressRequest request, ServerCallContext context)
    {
        Guid shopId = Guid.Parse(request.ShopId);

        AddressSnapshot? pickupAddress = await dbContext.Shops
            .Where(s => s.Id == shopId)
            .Select(s => s.PickupAddressSnapshot)
            .FirstOrDefaultAsync(context.CancellationToken);

        if (pickupAddress is null)
        {
            return new GetPickupAddressResponse
            {
                IsValid = false, FailureReason = "Shop does not exist or has not configured a pickup address."
            };
        }

        Common.Contracts.Grpc.Shop.AddressSnapshot snapshot = new()
        {
            UserId = pickupAddress.UserId.ToString(),
            FullName = pickupAddress.FullName,
            Phone = pickupAddress.Phone,
            Province = pickupAddress.Province,
            Ward = pickupAddress.Ward,
            AddressDetail = pickupAddress.AddressDetail,
            FullAddressText = pickupAddress.FullAddressText,
            AddressType = pickupAddress.AddressType
        };
        if (pickupAddress.Latitude.HasValue)
        {
            snapshot.Latitude = pickupAddress.Latitude.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (pickupAddress.Longitude.HasValue)
        {
            snapshot.Longitude = pickupAddress.Longitude.Value.ToString(CultureInfo.InvariantCulture);
        }

        return new GetPickupAddressResponse { IsValid = true, PickupAddressSnapshot = snapshot };
    }
}
