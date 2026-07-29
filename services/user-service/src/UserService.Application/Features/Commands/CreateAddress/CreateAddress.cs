namespace UserService.Application.Features.Commands.CreateAddress;

public record CreateAddressResponse(Guid AddressId);

public record CreateAddressCommand(
    string FullName,
    string Phone,
    string Province,
    string Ward,
    string AddressDetail,
    string FullAddressText,
    AddressType AddressType,
    bool IsDefault,
    bool IsPickUpAddress) : IRequest<CreateAddressResponse>;

public class CreateAddress(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<CreateAddressCommand, CreateAddressResponse>
{
    public async Task<CreateAddressResponse> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Guid userId = currentUser.UserId.Value;

        Address address = new()
        {
            UserId = userId,
            FullName = request.FullName,
            Phone = request.Phone,
            Province = request.Province,
            Ward = request.Ward,
            AddressDetail = request.AddressDetail,
            FullAddressText = request.FullAddressText,
            AddressType = request.AddressType,
            IsDefault = request.IsDefault,
            IsPickupAddress = request.IsPickUpAddress,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Addresses.Add(address);

        await context.SaveChangesAsync(cancellationToken);

        return new CreateAddressResponse(address.Id);
    }
}
