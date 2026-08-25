using System.Globalization;
using Common.Contracts.Grpc.User;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;
using UserService.Domain.Entities;

namespace UserService.API.Grpc;

public class UserGrpcServiceImpl(IApplicationDbContext dbContext)
    : UserGrpcService.UserGrpcServiceBase
{
    public override async Task<GetUserShippingAddressesResponse> GetUserShippingAddresses(
        GetUserShippingAddressesRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out Guid userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid user_id '{request.UserId}'."));
        }

        List<Address> addresses = await dbContext.Addresses
            .Where(a => a.UserId == userId)
            .ToListAsync(context.CancellationToken);

        GetUserShippingAddressesResponse response = new();

        response.Items.AddRange(addresses.Select(a =>
        {
            ShippingAddressItem item = new()
            {
                Id = a.Id.ToString(),
                FullName = a.FullName,
                Phone = a.Phone,
                Province = a.Province,
                Ward = a.Ward,
                AddressDetail = a.AddressDetail,
                FullAddressText = a.FullAddressText,
                AddressType = a.AddressType.ToString()
            };

            if (a.Latitude.HasValue)
            {
                item.Latitude = a.Latitude.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (a.Longitude.HasValue)
            {
                item.Longitude = a.Longitude.Value.ToString(CultureInfo.InvariantCulture);
            }

            return item;
        }));

        return response;
    }
}
