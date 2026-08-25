using System.Globalization;
using Common.Contracts.Grpc.Shipping;
using GrpcShippingFeeItem = Common.Contracts.Grpc.Shipping.ShippingFeeItem;
using AppShippingFeeItem = OrderService.Application.Common.Interfaces.ShippingFeeItem;

namespace OrderService.Infrastructure.Services;

public class ShippingServiceClient(
    ShippingGrpcService.ShippingGrpcServiceClient grpcClient) : IShippingServiceClient
{
    public async Task<ShippingFeeResult> CalculateFeeAsync(
        ShippingFeeRequest request, CancellationToken cancellationToken)
    {
        CalculateFeeRequest grpcRequest = new()
        {
            CarrierCode = request.CarrierCode,
            PickupProvince = request.PickupProvince,
            PickupWard = request.PickupWard,
            DeliveryProvince = request.DeliveryProvince,
            DeliveryWard = request.DeliveryWard
        };
        grpcRequest.Items.AddRange(request.Items.Select(ToGrpcShippingFeeItem));

        CalculateFeeResponse response = await grpcClient.CalculateFeeAsync(
            grpcRequest, deadline: DateTime.UtcNow.AddSeconds(3), cancellationToken: cancellationToken);

        if (!response.IsValid)
        {
            return new ShippingFeeResult(
                false, 0, null, null,
                response.HasFailureReason ? response.FailureReason : null);
        }

        return new ShippingFeeResult(
            true,
            response.Fee,
            response.HasEstimatedStart ? DateOnly.Parse(response.EstimatedStart, CultureInfo.InvariantCulture) : null,
            response.HasEstimatedEnd ? DateOnly.Parse(response.EstimatedEnd, CultureInfo.InvariantCulture) : null,
            null);
    }

    private static GrpcShippingFeeItem ToGrpcShippingFeeItem(AppShippingFeeItem item)
    {
        return new GrpcShippingFeeItem
        {
            Quantity = item.Quantity,
            WeightGram = item.WeightGram,
            Length = item.Length,
            Width = item.Width,
            Height = item.Height
        };
    }
}
