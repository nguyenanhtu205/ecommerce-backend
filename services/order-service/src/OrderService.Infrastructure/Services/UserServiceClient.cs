using System.Globalization;
using Common.Contracts.Grpc.User;
using OrderService.Domain.Common;

namespace OrderService.Infrastructure.Services;

public class UserServiceClient(
    UserGrpcService.UserGrpcServiceClient grpcClient) : IUserServiceClient
{
    public async Task<IReadOnlyCollection<UserShippingAddress>> GetUserShippingAddressesAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        GetUserShippingAddressesRequest request = new() { UserId = userId.ToString() };

        GetUserShippingAddressesResponse response = await grpcClient.GetUserShippingAddressesAsync(
            request, deadline: DateTime.UtcNow.AddSeconds(3), cancellationToken: cancellationToken);

        return
        [
            .. response.Items.Select(item => new UserShippingAddress(
                Guid.Parse(item.Id),
                new AddressSnapshot
                {
                    UserId = userId,
                    FullName = item.FullName,
                    Phone = item.Phone,
                    Province = item.Province,
                    Ward = item.Ward,
                    AddressDetail = item.AddressDetail,
                    FullAddressText = item.FullAddressText,
                    AddressType = item.AddressType,
                    Latitude = item.HasLatitude
                        ? decimal.Parse(item.Latitude, CultureInfo.InvariantCulture)
                        : null,
                    Longitude = item.HasLongitude
                        ? decimal.Parse(item.Longitude, CultureInfo.InvariantCulture)
                        : null
                }))
        ];
    }
}
