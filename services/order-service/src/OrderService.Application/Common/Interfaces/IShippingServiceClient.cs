namespace OrderService.Application.Common.Interfaces;

public interface IShippingServiceClient
{
    Task<ShippingFeeResult> CalculateFeeAsync(ShippingFeeRequest request, CancellationToken cancellationToken);
}

public record ShippingFeeRequest(
    Guid CarrierId,
    AddressSnapshot PickupAddressSnapshot,
    AddressSnapshot DeliveryAddressSnapshot);

public record ShippingFeeResult(
    bool IsValid,
    int Fee,
    DateOnly? EstimatedStart,
    DateOnly? EstimatedEnd,
    string? CarrierName,
    string? FailureReason);
