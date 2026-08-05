using SellerService.Domain.Common;

namespace SellerService.Application.Consumers;

public class PickupAddressSnapshotUpdatedConsumer(IApplicationDbContext db) : IConsumer<PickupAddressSnapshotUpdated>
{
    public async Task Consume(ConsumeContext<PickupAddressSnapshotUpdated> context)
    {
        PickupAddressSnapshotUpdated message = context.Message;

        Shop? shop = await db.Shops.Where(s => s.OwnerUserId == message.UserId)
            .FirstOrDefaultAsync(context.CancellationToken);

        if (shop == null)
        {
            throw new NotFoundException("Shop not found");
        }

        shop.PickupAddressSnapshot = new AddressSnapshot
        {
            UserId = message.UserId,
            FullName = message.FullName,
            Phone = message.Phone,
            Province = message.Province,
            Ward = message.Ward,
            AddressDetail = message.AddressDetail,
            FullAddressText = message.FullAddressText,
            Latitude = message.Latitude,
            Longitude = message.Longitude,
            AddressType = message.AddressType
        };

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
