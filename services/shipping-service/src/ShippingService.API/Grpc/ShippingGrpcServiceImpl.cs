using Common.Contracts.Grpc.Shipping;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ShippingService.Application.Common.Interfaces;
using ShippingService.Domain.Entities;

namespace ShippingService.API.Grpc;

public class ShippingGrpcServiceImpl(IApplicationDbContext dbContext, ICarrierAdapterFactory adapterFactory)
    : ShippingGrpcService.ShippingGrpcServiceBase
{
    public override async Task<CalculateFeeResponse> CalculateFee(CalculateFeeRequest request, 
        ServerCallContext context)
    {
        Carrier? carrier = await dbContext.Carriers
            .FirstOrDefaultAsync(c => c.Code == request.CarrierCode, context.CancellationToken);

        if (carrier is null)
        {
            return new CalculateFeeResponse
            {
                IsValid = false, FailureReason = $"Carrier '{request.CarrierCode}' does not exist."
            };
        }

        if (request.Items.Count == 0)
        {
            return new CalculateFeeResponse { IsValid = false, FailureReason = "Items must not be empty." };
        }

        int totalWeight = request.Items.Sum(i => i.WeightGram * i.Quantity);
        int packageLength = request.Items.Max(i => i.Length);
        int packageWidth = request.Items.Max(i => i.Width);
        int packageHeight = request.Items.Sum(i => i.Height * i.Quantity);

        ICarrierShippingAdapter adapter = adapterFactory.GetAdapter(carrier.Code);
        CarrierFeeResult result = await adapter.CalculateFeeAsync(
            new CarrierShippingRequest(
                request.PickupProvince, request.PickupWard,
                request.DeliveryProvince, request.DeliveryWard,
                totalWeight, packageLength, packageWidth, packageHeight),
            context.CancellationToken);

        if (!result.IsValid)
        {
            return new CalculateFeeResponse { IsValid = false, FailureReason = result.FailureReason };
        }

        return new CalculateFeeResponse
        {
            IsValid = true,
            Fee = result.Fee,
            EstimatedStart = result.EstimatedStart?.ToString("yyyy-MM-dd"),
            EstimatedEnd = result.EstimatedEnd?.ToString("yyyy-MM-dd")
        };
    }
}
