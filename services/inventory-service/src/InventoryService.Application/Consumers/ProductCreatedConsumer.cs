using Npgsql;

namespace InventoryService.Application.Consumers;

public class ProductCreatedConsumer(IApplicationDbContext db) : IConsumer<ProductCreated>
{
    public async Task Consume(ConsumeContext<ProductCreated> context)
    {
        ProductCreated message = context.Message;

        string eventId = $"{nameof(ProductCreated)}-{message.ProductId}-{message.CreatedAt:O}";

        db.ProcessedEvents.Add(new ProcessedEvent
        {
            EventId = eventId, EventType = nameof(ProductCreated), ProcessedAt = DateTimeOffset.UtcNow
        });

        foreach (VariantCombinationInit combination in message.VariantCombinations)
        {
            db.ProductVariantCombinations.Add(new ProductVariantCombination
            {
                Id = Guid.Parse(combination.CombinationId),
                ProductId = Guid.Parse(message.ProductId),
                ShopId = Guid.Parse(message.ShopId),
                Sku = combination.Sku,
                Price = combination.InitialPrice,
                Stock = combination.InitialStock,
                ReservedStock = 0,
                Version = 0,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        try
        {
            await db.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException { SqlState: "23505" };
    }
}
