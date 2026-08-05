namespace ShippingService.Domain.Common;

public class AddressSnapshot
{
    public required Guid UserId { get; init; }

    public required string FullName { get; set; }

    public required string Phone { get; set; }

    public required string Province { get; set; }

    public required string Ward { get; set; }

    public required string AddressDetail { get; set; }

    public required string FullAddressText { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public required string AddressType { get; set; }
}
