using InventoryService.Domain.Enums;

namespace InventoryService.Application.Consumers;

public class ReserveStockConsumer(
    IApplicationDbContext dbContext,
    ITopicProducer<StockReserved> stockReservedProducer,
    ITopicProducer<StockReservationFailed> stockReservationFailedProducer) : IConsumer<ReserveStock>
{
    private const int ReservationTtlMinutes = 15;
    private const int MaxConcurrencyRetries = 3;

    public async Task Consume(ConsumeContext<ReserveStock> context)
    {
        string eventId = $"{nameof(ReserveStock)}-{context.Message.OrderId}";

        if (await dbContext.ProcessedEvents.AnyAsync(e => e.EventId == eventId, context.CancellationToken))
        {
            return;
        }

        for (int attempt = 0; attempt < MaxConcurrencyRetries; attempt++)
        {
            string? failureReason = null;

            foreach (ReserveStockItem item in context.Message.Items)
            {
                ProductVariantCombination? combination = await dbContext.ProductVariantCombinations
                    .FirstOrDefaultAsync(c => c.Id == item.CombinationId, context.CancellationToken);

                if (combination is null)
                {
                    failureReason = $"Combination {item.CombinationId} does not exist.";
                    break;
                }

                int availableStock = combination.Stock - combination.ReservedStock;
                if (availableStock < item.Quantity)
                {
                    failureReason =
                        $"Combination {item.CombinationId} has insufficient stock (available: {availableStock}, required: {item.Quantity}).";
                    break;
                }

                combination.ReservedStock += item.Quantity;
                combination.Version += 1;
                combination.UpdatedAt = DateTimeOffset.UtcNow;

                dbContext.StockReservations.Add(new StockReservation
                {
                    CombinationId = item.CombinationId,
                    OrderId = context.Message.OrderId,
                    Quantity = item.Quantity,
                    Status = StockReservationStatus.Reserved,
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(ReservationTtlMinutes),
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            if (failureReason is not null)
            {
                await stockReservationFailedProducer.Produce(
                    new StockReservationFailed(context.Message.OrderId, failureReason), context.CancellationToken);
                return;
            }

            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                EventId = eventId, EventType = nameof(ReserveStock), ProcessedAt = DateTimeOffset.UtcNow
            });

            try
            {
                await dbContext.SaveChangesAsync(context.CancellationToken);
                await stockReservedProducer.Produce(
                    new StockReserved(context.Message.OrderId), context.CancellationToken);
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
