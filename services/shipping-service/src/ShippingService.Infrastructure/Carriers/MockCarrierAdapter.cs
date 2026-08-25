namespace ShippingService.Infrastructure.Carriers;

public class MockCarrierAdapter : ICarrierShippingAdapter
{
    public string CarrierCode => "mock";

    public Task<CarrierFeeResult> CalculateFeeAsync(CarrierShippingRequest request, CancellationToken cancellationToken)
    {
        int volumeWeight = request.Length * request.Width * request.Height / 5;
        int chargeableWeight = Math.Max(request.WeightGram, volumeWeight);
        int fee = request.PickupProvince == request.DeliveryProvince ? 15000 : 25000;
        fee += chargeableWeight / 1000 * 2000;

        return Task.FromResult(new CarrierFeeResult(
            true, fee,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMinutes(2)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddMinutes(5)),
            null));
    }

    public Task<CarrierCreateOrderResult> CreateOrderAsync(CarrierCreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new CarrierCreateOrderResult(
            true, $"MOCK-{request.OrderId:N}"[..16],
            DateOnly.FromDateTime(DateTime.UtcNow.AddMinutes(2)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddMinutes(5)),
            null));
    }
}
