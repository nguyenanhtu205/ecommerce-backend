using ShippingService.Domain.Common;

namespace ShippingService.Application.Common.Mappers;

public static class AddressMapper
{
    public static AddressSnapshot ToDomain(CheckoutAddressSnapshot checkoutAddressSnapshot)
    {
        return new AddressSnapshot
        {
            AddressDetail = checkoutAddressSnapshot.AddressDetail,
            AddressType = checkoutAddressSnapshot.AddressType,
            FullAddressText = checkoutAddressSnapshot.FullAddressText,
            FullName = checkoutAddressSnapshot.FullName,
            Latitude = checkoutAddressSnapshot.Latitude,
            Longitude = checkoutAddressSnapshot.Longitude,
            Phone = checkoutAddressSnapshot.Phone,
            Province = checkoutAddressSnapshot.Province,
            UserId = checkoutAddressSnapshot.UserId,
            Ward = checkoutAddressSnapshot.Ward
        };
    }
}
