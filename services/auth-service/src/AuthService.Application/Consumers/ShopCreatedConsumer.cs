namespace AuthService.Application.Consumers;

public class ShopCreatedConsumer(IApplicationDbContext db) : IConsumer<ShopCreated>
{
    public async Task Consume(ConsumeContext<ShopCreated> context)
    {
        ShopCreated message = context.Message;

        User? user = await db.Users.FindAsync([message.SellerId], context.CancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.ShopId = message.ShopId;
        user.ShopName = message.ShopName;

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
