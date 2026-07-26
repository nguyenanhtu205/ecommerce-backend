namespace UserService.Application.Consumers;

public class UserRegisteredConsumer(IApplicationDbContext db) : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        UserRegisteredEvent message = context.Message;

        bool exists = await db.Profiles.AnyAsync(
            p => p.Id == message.UserId, context.CancellationToken);

        if (exists)
        {
            return;
        }

        db.Profiles.Add(new Profile
        {
            Id = message.UserId,
            DisplayName = message.Email,
            CreatedAt = message.RegisteredAt,
            UpdatedAt = message.RegisteredAt
        });

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
