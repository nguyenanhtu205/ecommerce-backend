using System.Globalization;
using Common.Contracts.Grpc.Shop;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using SellerService.Application.Common.Interfaces;
using SellerService.Domain.Entities;

namespace SellerService.API.Grpc;

public class ShopGrpcServiceImpl(IApplicationDbContext dbContext) : ShopGrpcService.ShopGrpcServiceBase
{
    public override async Task<GetPickupAddressResponse> GetPickupAddress(
        GetPickupAddressRequest request, ServerCallContext context)
    {
        Guid shopId = Guid.Parse(request.ShopId);

        Shop? shop = await dbContext.Shops
            .AsNoTracking()
            .Where(s => s.Id == shopId)
            .FirstOrDefaultAsync(context.CancellationToken);

        if (shop is null)
        {
            return new GetPickupAddressResponse
            {
                IsValid = false,
                ShopName = "Unknown",
                FailureReason = "Shop does not exist or has not configured a pickup address."
            };
        }

        AddressSnapshot snapshot = new()
        {
            UserId = shop.PickupAddressSnapshot.UserId.ToString(),
            FullName = shop.PickupAddressSnapshot.FullName,
            Phone = shop.PickupAddressSnapshot.Phone,
            Province = shop.PickupAddressSnapshot.Province,
            Ward = shop.PickupAddressSnapshot.Ward,
            AddressDetail = shop.PickupAddressSnapshot.AddressDetail,
            FullAddressText = shop.PickupAddressSnapshot.FullAddressText,
            AddressType = shop.PickupAddressSnapshot.AddressType
        };
        if (shop.PickupAddressSnapshot.Latitude.HasValue)
        {
            snapshot.Latitude = shop.PickupAddressSnapshot.Latitude.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (shop.PickupAddressSnapshot.Longitude.HasValue)
        {
            snapshot.Longitude = shop.PickupAddressSnapshot.Longitude.Value.ToString(CultureInfo.InvariantCulture);
        }

        return new GetPickupAddressResponse { IsValid = true, PickupAddressSnapshot = snapshot, ShopName = shop.Name };
    }
}
