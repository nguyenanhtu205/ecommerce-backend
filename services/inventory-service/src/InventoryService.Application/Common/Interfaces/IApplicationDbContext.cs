using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace InventoryService.Application.Common.Interfaces;

public interface IApplicationDbContext : IOutboxDbContext
{
    DbSet<ProductVariantCombination> ProductVariantCombinations { get; }

    DbSet<StockReservation> StockReservations { get; }

    ChangeTracker ChangeTracker { get; }
}
