using System.Text.Json;

namespace Common.Outbox;

public interface IOutboxWriter
{
    void Enqueue<T>(T message) where T : class;
}

public class OutboxWriter(IOutboxDbContext dbContext) : IOutboxWriter
{
    public void Enqueue<T>(T message) where T : class
    {
        dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            MessageType = typeof(T).FullName!,
            Payload = JsonSerializer.Serialize(message),
            CreatedAt = DateTimeOffset.UtcNow,
            AttemptCount = 0
        });
    }
}
