using Common.Contracts.Grpc.Shipping;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ShippingService.Application.Common.Interfaces;
using ShippingService.Domain.Entities;

namespace ShippingService.API.Grpc;

public class ShippingGrpcServiceImpl(IApplicationDbContext dbContext) : ShippingGrpcService.ShippingGrpcServiceBase
{
    public override async Task<CalculateFeeResponse> CalculateFee(
        CalculateFeeRequest request, ServerCallContext context)
    {
        Guid carrierId = Guid.Parse(request.CarrierId);

        Carrier? carrier = await dbContext.Carriers
            .FirstOrDefaultAsync(c => c.Id == carrierId, context.CancellationToken);

        if (carrier is null)
        {
            return new CalculateFeeResponse { IsValid = false, FailureReason = "Carrier does not exist." };
        }

        // TODO: thay bằng call thật sang API carrier (GHN/GHTK/J&T...) để tính phí
        int fee = EstimateFee(carrier.Code, request.PickupAddressSnapshot, request.DeliveryAddressSnapshot);
        DateOnly estimatedStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        DateOnly estimatedEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));

        return new CalculateFeeResponse
        {
            IsValid = true,
            Fee = fee,
            EstimatedStart = estimatedStart.ToString("yyyy-MM-dd"),
            EstimatedEnd = estimatedEnd.ToString("yyyy-MM-dd"),
            CarrierName = carrier.Name
        };
    }

    private static int EstimateFee(
        string carrierCode,
        AddressSnapshot pickup,
        AddressSnapshot delivery)
    {
        int baseFee = carrierCode switch
        {
            "ghn" => 20000,
            "ghtk" => 18000,
            "jnt" => 19000,
            _ => 25000
        };

        return pickup.Province == delivery.Province ? baseFee : baseFee + 10000;
    }
}
