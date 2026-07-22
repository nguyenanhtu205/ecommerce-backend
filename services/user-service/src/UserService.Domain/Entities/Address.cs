namespace UserService.Domain.Entities;

public class Address : BaseEntity
{
    public required Guid UserId { get; init; }

    public required string FullName { get; init; }

    public required string Phone { get; init; }

    public required string Province { get; init; }

    public required string Ward { get; init; }

    public required string AddressDetail { get; init; }

    public required string FullAddressText { get; init; }

    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }

    public required AddressType AddressType { get; set; }

    public bool IsDefault { get; set; } = false;

    public bool IsPickupAddress { get; set; } = false;

    public required DateTimeOffset CreatedAt { get; init; }

    public Profile? Profile { get; init; }
}
