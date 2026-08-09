using InventoryService.Domain.Enums;

namespace InventoryService.Application.Consumers;

public class ReleaseStockCommandConsumer(IApplicationDbContext dbContext) : IConsumer<ReleaseStockCommand>
{
    private const int MaxConcurrencyRetries = 3;

    public async Task Consume(ConsumeContext<ReleaseStockCommand> context)
    {
        string eventId = $"{nameof(ReleaseStockCommand)}-{context.Message.OrderId}";

        if (await dbContext.ProcessedEvents.AnyAsync(e => e.EventId == eventId, context.CancellationToken))
        {
            return;
        }

        for (int attempt = 0; attempt < MaxConcurrencyRetries; attempt++)
        {
            List<StockReservation> reservations = await dbContext.StockReservations
                .Where(r => r.OrderId == context.Message.OrderId && r.Status == StockReservationStatus.Reserved)
                .ToListAsync(context.CancellationToken);

            foreach (StockReservation reservation in reservations)
            {
                ProductVariantCombination? combination = await dbContext.ProductVariantCombinations
                    .FirstOrDefaultAsync(c => c.Id == reservation.CombinationId, context.CancellationToken);

                if (combination is not null)
                {
                    combination.ReservedStock = Math.Max(0, combination.ReservedStock - reservation.Quantity);
                    combination.Version += 1;
                    combination.UpdatedAt = DateTimeOffset.UtcNow;
                }

                reservation.Status = StockReservationStatus.Released;
            }

            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                EventId = eventId, EventType = nameof(ReleaseStockCommand), ProcessedAt = DateTimeOffset.UtcNow
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
