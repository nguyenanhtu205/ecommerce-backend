using ShippingService.Domain.Common;

namespace ShippingService.Application.Common.Interfaces;

public record CarrierShippingRequest(
    string PickupProvince,
    string PickupWard,
    string DeliveryProvince,
    string DeliveryWard,
    int WeightGram,
    int Length,
    int Width,
    int Height,
    int InsuranceValue = 0);

public record CarrierFeeResult(
    bool IsValid,
    int Fee,
    DateOnly? EstimatedStart,
    DateOnly? EstimatedEnd,
    string? FailureReason);

public record CarrierCreateOrderRequest(
    Guid OrderId,
    AddressSnapshot Pickup,
    AddressSnapshot Delivery,
    int WeightGram,
    int InsuranceValue,
    string? Note);

public record CarrierCreateOrderResult(
    bool Success,
    string? TrackingCode,
    DateOnly? EstimatedStart,
    DateOnly? EstimatedEnd,
    string? FailureReason);

public interface ICarrierShippingAdapter
{
    string CarrierCode { get; }
    
    Task<CarrierFeeResult> CalculateFeeAsync(CarrierShippingRequest request, CancellationToken cancellationToken);

    Task<CarrierCreateOrderResult> CreateOrderAsync(CarrierCreateOrderRequest request,
        CancellationToken cancellationToken);
}

public interface ICarrierAdapterFactory
{
    ICarrierShippingAdapter GetAdapter(string carrierCode);
}
