namespace ShippingService.Infrastructure.Carriers;

public class CarrierAdapterFactory(IEnumerable<ICarrierShippingAdapter> adapters) : ICarrierAdapterFactory
{
    public ICarrierShippingAdapter GetAdapter(string carrierCode)
    {
        ICarrierShippingAdapter? adapter = adapters.FirstOrDefault(a =>
            string.Equals(a.CarrierCode, carrierCode, StringComparison.OrdinalIgnoreCase));

        return adapter ??
               throw new InvalidOperationException($"No adapter available for the carrier code '{carrierCode}'.");
    }
}
