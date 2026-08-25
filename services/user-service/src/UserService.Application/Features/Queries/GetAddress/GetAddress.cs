namespace UserService.Application.Features.Queries.GetAddress;

public record AddressDto(
    string Id,
    string FullName,
    string Phone,
    string Province,
    string Ward,
    string AddressDetail,
    string FullAddressText,
    decimal? Latitude,
    decimal? Longitude,
    AddressType AddressType,
    bool IsDefault,
    bool IsPickupAddress,
    DateTimeOffset CreatedAt);

public record GetAddressQuery : IRequest<List<AddressDto>>;

public class GetAddress(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetAddressQuery, List<AddressDto>>
{
    public async Task<List<AddressDto>> Handle(GetAddressQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        List<Address> addresses = await context.Addresses
            .AsNoTracking()
            .Where(x => x.UserId == currentUser.UserId.Value)
            .ToListAsync(cancellationToken);

        return
        [
            .. addresses.Select(a => new AddressDto(a.Id.ToString(), a.FullName, a.Phone, a.Province, a.Ward,
                a.AddressDetail, a.FullAddressText, a.Latitude, a.Longitude, a.AddressType, a.IsDefault,
                a.IsPickupAddress, a.CreatedAt))
        ];
    }
}
