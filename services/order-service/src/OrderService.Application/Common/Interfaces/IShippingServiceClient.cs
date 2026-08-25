namespace OrderService.Application.Common.Interfaces;

public interface IShippingServiceClient
{
    Task<ShippingFeeResult> CalculateFeeAsync(ShippingFeeRequest request, CancellationToken cancellationToken);
}

public record ShippingFeeItem(int Quantity, int WeightGram, int Length, int Width, int Height);

public record ShippingFeeRequest(
    string CarrierCode,
    string PickupProvince,
    string PickupWard,
    string DeliveryProvince,
    string DeliveryWard,
    List<ShippingFeeItem> Items);

public record ShippingFeeResult(
    bool IsValid,
    int Fee,
    DateOnly? EstimatedStart,
    DateOnly? EstimatedEnd,
    string? FailureReason);
