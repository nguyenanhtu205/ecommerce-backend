using Microsoft.EntityFrameworkCore;

namespace Common.Outbox;

public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }

    DbSet<ProcessedEvent> ProcessedEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
