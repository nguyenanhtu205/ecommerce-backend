namespace OrderService.Application.Common.Mappers;

public static class AddressMapper
{
    public static CheckoutAddressSnapshot ToCheckoutAddressSnapshot(AddressSnapshot addressSnapshot)
    {
        return new CheckoutAddressSnapshot
        (
            addressSnapshot.UserId,
            addressSnapshot.FullName,
            addressSnapshot.Phone,
            addressSnapshot.Province,
            addressSnapshot.Ward,
            addressSnapshot.AddressDetail,
            addressSnapshot.FullAddressText,
            addressSnapshot.Latitude,
            addressSnapshot.Longitude,
            addressSnapshot.AddressType
        );
    }

    public static AddressSnapshot ToAddressSnapshot(CheckoutAddressSnapshot checkoutAddressSnapshot)
    {
        return new AddressSnapshot
        {
            UserId = checkoutAddressSnapshot.UserId,
            FullName = checkoutAddressSnapshot.FullName,
            Phone = checkoutAddressSnapshot.Phone,
            Province = checkoutAddressSnapshot.Province,
            Ward = checkoutAddressSnapshot.Ward,
            AddressDetail = checkoutAddressSnapshot.AddressDetail,
            FullAddressText = checkoutAddressSnapshot.FullAddressText,
            Latitude = checkoutAddressSnapshot.Latitude,
            Longitude = checkoutAddressSnapshot.Longitude,
            AddressType = checkoutAddressSnapshot.AddressType
        };
    }
}
