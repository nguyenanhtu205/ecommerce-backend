using System.Globalization;
using Common.Contracts.Grpc.Shipping;
using AddressSnapshot = OrderService.Domain.Common.AddressSnapshot;
using GrpcAddressSnapshot = Common.Contracts.Grpc.Shipping.AddressSnapshot;

namespace OrderService.Infrastructure.Services;

public class ShippingServiceClient(
    ShippingGrpcService.ShippingGrpcServiceClient grpcClient) : IShippingServiceClient
{
    public async Task<ShippingFeeResult> CalculateFeeAsync(
        ShippingFeeRequest request, CancellationToken cancellationToken)
    {
        CalculateFeeRequest grpcRequest = new()
        {
            CarrierId = request.CarrierId.ToString(),
            PickupAddressSnapshot = ToGrpc(request.PickupAddressSnapshot),
            DeliveryAddressSnapshot = ToGrpc(request.DeliveryAddressSnapshot)
        };

        CalculateFeeResponse response = await grpcClient.CalculateFeeAsync(
            grpcRequest, deadline: DateTime.UtcNow.AddSeconds(3), cancellationToken: cancellationToken);

        if (!response.IsValid)
        {
            return new ShippingFeeResult(
                false, 0, null, null, null,
                response.HasFailureReason ? response.FailureReason : null);
        }

        return new ShippingFeeResult(
            true,
            response.Fee,
            response.HasEstimatedStart ? DateOnly.Parse(response.EstimatedStart, CultureInfo.InvariantCulture) : null,
            response.HasEstimatedEnd ? DateOnly.Parse(response.EstimatedEnd, CultureInfo.InvariantCulture) : null,
            response.HasCarrierName ? response.CarrierName : null,
            null);
    }

    private static GrpcAddressSnapshot ToGrpc(AddressSnapshot address)
    {
        GrpcAddressSnapshot snapshot = new()
        {
            UserId = address.UserId.ToString(),
            FullName = address.FullName,
            Phone = address.Phone,
            Province = address.Province,
            Ward = address.Ward,
            AddressDetail = address.AddressDetail,
            FullAddressText = address.FullAddressText,
            AddressType = address.AddressType
        };
        if (address.Latitude.HasValue)
        {
            snapshot.Latitude = address.Latitude.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (address.Longitude.HasValue)
        {
            snapshot.Longitude = address.Longitude.Value.ToString(CultureInfo.InvariantCulture);
        }

        return snapshot;
    }
}
