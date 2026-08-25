using InventoryService.Domain.Enums;

namespace InventoryService.Application.Consumers;

public class CommitStockCommandConsumer(IApplicationDbContext dbContext, IOutboxWriter outboxWriter)
    : IConsumer<CommitStockCommand>
{
    private const int MaxConcurrencyRetries = 3;

    public async Task Consume(ConsumeContext<CommitStockCommand> context)
    {
        string eventId = $"{nameof(CommitStockCommand)}-{context.Message.OrderId}";

        if (await dbContext.ProcessedEvents.AnyAsync(e => e.EventId == eventId, context.CancellationToken))
        {
            return;
        }

        for (int attempt = 0; attempt < MaxConcurrencyRetries; attempt++)
        {
            List<StockReservation> reservations = await dbContext.StockReservations
                .Where(r => r.OrderId == context.Message.OrderId && r.Status == StockReservationStatus.Reserved)
                .ToListAsync(context.CancellationToken);

            List<StockCommitedItem> committedItems = new();

            foreach (StockReservation reservation in reservations)
            {
                ProductVariantCombination? combination = await dbContext.ProductVariantCombinations
                    .FirstOrDefaultAsync(c => c.Id == reservation.CombinationId, context.CancellationToken);

                if (combination is not null)
                {
                    combination.Stock = Math.Max(0, combination.Stock - reservation.Quantity);
                    combination.ReservedStock = Math.Max(0, combination.ReservedStock - reservation.Quantity);
                    combination.Version += 1;
                    combination.UpdatedAt = DateTimeOffset.UtcNow;

                    committedItems.Add(new StockCommitedItem(
                        combination.ProductId,
                        combination.Id,
                        reservation.Quantity));
                }

                reservation.Status = StockReservationStatus.Commited;
            }

            if (committedItems.Count > 0)
            {
                outboxWriter.Enqueue(new StockCommited(committedItems));
            }

            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                EventType = nameof(CommitStockCommand),
                ProcessedAt = DateTimeOffset.UtcNow
            });

            try
            {
                await dbContext.SaveChangesAsync(context.CancellationToken);
                return;
            }
            catch (DbUpdateConcurrencyException)
            {
                dbContext.ChangeTracker.Clear();

                if (attempt == MaxConcurrencyRetries - 1)
                {
                    throw;
                }
            }
        }
    }
}
