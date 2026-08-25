namespace SellerService.Infrastructure.Data.Configurations;

public class ShopShippingCarrierConnectionsConfiguration : IEntityTypeConfiguration<ShopShippingCarrierConnection>
{
    public void Configure(EntityTypeBuilder<ShopShippingCarrierConnection> builder)
    {
        builder.ToTable("shop_shipping_carrier_connections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id")
            .IsRequired();

        builder.Property(x => x.CarrierId)
            .HasColumnName("carrier_id")
            .IsRequired();

        builder.Property(x => x.CarrierCode)
            .HasColumnName("carrier_code")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ConnectedAt)
            .HasColumnName("connected_at")
            .HasColumnType("timestamptz");

        builder.HasIndex(x => new { x.ShopId, x.CarrierId })
            .IsUnique();

        builder.HasOne(x => x.Shop)
            .WithMany(x => x.ShippingCarrierConnections)
            .HasForeignKey(x => x.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
