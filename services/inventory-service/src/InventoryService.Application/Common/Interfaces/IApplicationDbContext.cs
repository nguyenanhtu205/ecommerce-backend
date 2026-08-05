using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace InventoryService.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ProductVariantCombination> ProductVariantCombinations { get; }

    DbSet<StockReservation> StockReservations { get; }

    DbSet<ProcessedEvent> ProcessedEvents { get; }

    ChangeTracker ChangeTracker { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
