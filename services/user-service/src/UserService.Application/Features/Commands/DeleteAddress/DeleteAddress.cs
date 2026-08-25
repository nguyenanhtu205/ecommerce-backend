namespace UserService.Application.Features.Commands.DeleteAddress;

public record DeleteAddressCommand(string AddressId) : IRequest;

public class DeleteAddress(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<DeleteAddressCommand>
{
    public async Task Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
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

        bool wasDefault = address.IsDefault;
        Guid userId = address.UserId;

        context.Addresses.Remove(address);

        if (wasDefault)
        {
            Address? latestAddress = await context.Addresses
                .Where(a => a.UserId == userId && a.Id != address.Id)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            latestAddress?.IsDefault = true;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
