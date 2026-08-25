namespace OrderService.Application.Common.Interfaces;

public interface IUserServiceClient
{
    Task<IReadOnlyCollection<UserShippingAddress>> GetUserShippingAddressesAsync(Guid userId,
        CancellationToken cancellationToken);
}

public record UserShippingAddress(Guid Id, AddressSnapshot ShippingAddressSnapshot);
