namespace ShippingService.Application.Features.Commands.CalculateShippingFee;

public record ShippingFeeItem(string CombinationId, int Quantity, int WeightGram, int Length, int Width, int Height);

public record ShopShippingFeeRequest(
    string ShopId,
    string CarrierCode,
    string PickupProvince,
    string PickupWard,
    List<ShippingFeeItem> Items);

public record CalculateShippingFeeCommand(
    string DeliveryProvince,
    string DeliveryWard,
    List<ShopShippingFeeRequest> Shops)
    : IRequest<List<CalculateShippingFeeItemResult>>;

public record CalculateShippingFeeItemResult(
    string ShopId,
    string CombinationId,
    bool IsValid,
    int Fee,
    DateOnly? EstimatedStart,
    DateOnly? EstimatedEnd,
    string? FailureReason);

public class CalculateShippingFeeCommandHandler(IApplicationDbContext context, ICarrierAdapterFactory adapterFactory)
    : IRequestHandler<CalculateShippingFeeCommand, List<CalculateShippingFeeItemResult>>
{
    public async Task<List<CalculateShippingFeeItemResult>> Handle(
        CalculateShippingFeeCommand request, CancellationToken cancellationToken)
    {
        if (request.Shops.Count == 0)
        {
            return [];
        }

        List<string> carrierCodes = [.. request.Shops.Select(s => s.CarrierCode).Distinct()];
        Dictionary<string, Carrier> carrierByCode = await context.Carriers
            .Where(c => carrierCodes.Contains(c.Code))
            .ToDictionaryAsync(c => c.Code, cancellationToken);

        List<CalculateShippingFeeItemResult> results = [];

        List<ShopShippingFeeRequest> validShops = [.. request.Shops.Where(shop => shop.Items.Count > 0)];

        foreach (ShopShippingFeeRequest shop in validShops)
        {
            if (!carrierByCode.TryGetValue(shop.CarrierCode, out Carrier? carrier))
            {
                results.AddRange(shop.Items.Select(i => new CalculateShippingFeeItemResult(
                    shop.ShopId, i.CombinationId, false, 0, null, null,
                    $"Carrier '{shop.CarrierCode}' does not exist.")));
                continue;
            }

            int totalWeight = shop.Items.Sum(i => i.WeightGram * i.Quantity);
            int packageLength = shop.Items.Max(i => i.Length);
            int packageWidth = shop.Items.Max(i => i.Width);
            int packageHeight = shop.Items.Sum(i => i.Height * i.Quantity);

            ICarrierShippingAdapter adapter = adapterFactory.GetAdapter(carrier.Code);

            CarrierFeeResult feeResult = await adapter.CalculateFeeAsync(
                new CarrierShippingRequest(
                    shop.PickupProvince, shop.PickupWard,
                    request.DeliveryProvince, request.DeliveryWard,
                    totalWeight, packageLength, packageWidth, packageHeight),
                cancellationToken);

            results.AddRange(shop.Items.Select(i => new CalculateShippingFeeItemResult(
                shop.ShopId, i.CombinationId, feeResult.IsValid, feeResult.Fee,
                feeResult.EstimatedStart, feeResult.EstimatedEnd, feeResult.FailureReason)));
        }

        return results;
    }
}
