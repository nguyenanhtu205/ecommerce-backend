namespace UserService.Application.Features.Commands.UpdateAddress;

public record UpdateAddressCommand(
    string Id,
    string? FullName,
    string? Phone,
    string? Province,
    string? Ward,
    string? AddressDetail,
    string? FullAddressText,
    decimal? Latitude,
    decimal? Longitude,
    AddressType? AddressType) : IRequest;

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

        Address? address = await context.Addresses.FindAsync([Guid.Parse(request.Id)], cancellationToken);

        if (address == null)
        {
            throw new NotFoundException("Address not found");
        }

        if (address.UserId != userId)
        {
            throw new ForbiddenAccessException();
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

        await context.SaveChangesAsync(cancellationToken);
    }
}
