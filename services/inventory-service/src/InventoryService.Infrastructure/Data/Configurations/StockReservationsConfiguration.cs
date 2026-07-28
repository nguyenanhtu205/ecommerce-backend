namespace InventoryService.Infrastructure.Data.Configurations;

public class StockReservationsConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("stock_reservations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.CombinationId)
            .HasColumnName("combination_id")
            .IsRequired();

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => new { x.OrderId, x.CombinationId })
            .IsUnique();

        builder.HasIndex(x => x.CombinationId);

        builder.HasIndex(x => new { x.Status, x.ExpiresAt });

        builder.HasOne(x => x.ProductVariantCombination)
            .WithMany(x => x.StockReservations)
            .HasForeignKey(x => x.CombinationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
