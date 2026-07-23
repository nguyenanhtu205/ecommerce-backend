namespace UserService.Application.Features.Commands.UpdateAddress;

public record UpdateAddressCommand(
    Guid Id,
    string? FullName,
    string? Phone,
    string? Province,
    string? Ward,
    string? AddressDetail,
    string? FullAddressText,
    decimal? Latitude,
    decimal? Longitude,
    AddressType? AddressType,
    bool? IsDefault,
    bool? IsPickupAddress) : IRequest;

public class UpdateAddress(
    IApplicationDbContext context,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateAddressCommand>
{
    public async Task Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        Guid? userId = currentUser.UserId;

        if (userId == null)
        {
            throw new UnauthorizedAccessException();
        }

        Address? address = await context.Addresses
            .FirstOrDefaultAsync(
                x => x.Id == request.Id && x.UserId == userId,
                cancellationToken);

        if (address == null)
        {
            throw new NotFoundException("Address not found");
        }

        address.FullName = request.FullName ?? address.FullName;
        address.Phone = request.Phone ?? address.Phone;
        address.Province = request.Province ?? address.Province;
        address.Ward = request.Ward ?? address.Ward;
        address.AddressDetail = request.AddressDetail ?? address.AddressDetail;
        address.FullAddressText = request.FullAddressText ?? address.FullAddressText;
        address.Latitude = request.Latitude ?? address.Latitude;
        address.Longitude = request.Longitude ?? address.Longitude;
        address.AddressType = request.AddressType ?? address.AddressType;
        address.IsPickupAddress = request.IsPickupAddress ?? address.IsPickupAddress;
        
        if (request.IsDefault == true)
        {
            List<Address> defaultAddresses = await context.Addresses
                .Where(x => x.UserId == userId && x.Id != address.Id && x.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (Address item in defaultAddresses)
            {
                item.IsDefault = false;
            }

            address.IsDefault = true;
        }
        else if (request.IsDefault.HasValue)
        {
            address.IsDefault = request.IsDefault.Value;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
