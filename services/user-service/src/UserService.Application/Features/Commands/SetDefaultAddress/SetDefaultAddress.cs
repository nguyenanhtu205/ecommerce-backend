namespace UserService.Application.Features.Commands.SetDefaultAddress;

public record SetDefaultAddressCommand(string AddressId) : IRequest;

public class SetDefaultAddress(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<SetDefaultAddressCommand>
{
    public async Task Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Address? address = await context.Addresses.FindAsync([Guid.Parse(request.AddressId)], cancellationToken);

        if (address is null)
        {
            throw new NotFoundException("Address not found");
        }

        if (address.UserId != currentUser.UserId.Value)
        {
            throw new ForbiddenAccessException();
        }

        List<Address> defaultAddresses = await context.Addresses
            .Where(a => a.IsDefault == true && a.UserId == currentUser.UserId.Value)
            .ToListAsync(cancellationToken);

        foreach (Address defaultAddress in defaultAddresses)
        {
            defaultAddress.IsDefault = false;
        }

        address.IsDefault = true;

        await context.SaveChangesAsync(cancellationToken);
    }
}
