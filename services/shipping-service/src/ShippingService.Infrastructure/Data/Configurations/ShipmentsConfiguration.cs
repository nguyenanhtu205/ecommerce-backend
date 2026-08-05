namespace ShippingService.Infrastructure.Data.Configurations;

public class ShipmentsConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("shipments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.CarrierId)
            .HasColumnName("carrier_id")
            .IsRequired();

        builder.Property(x => x.TrackingCode)
            .HasMaxLength(255)
            .HasColumnName("tracking_code");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(ShipmentStatus.Pending)
            .IsRequired();

        builder.Property(x => x.PickupAddressSnapshot)
            .HasColumnName("pickup_address_snapshot")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.DeliveryAddressSnapshot)
            .HasColumnName("delivery_address_snapshot")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.EstimatedDeliveryStart)
            .HasColumnName("estimated_delivery_start")
            .HasColumnType("date");

        builder.Property(x => x.EstimatedDeliveryEnd)
            .HasColumnName("estimated_delivery_end")
            .HasColumnType("date");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => x.OrderId)
            .IsUnique();

        builder.HasOne(x => x.Carrier)
            .WithMany(x => x.Shipments)
            .HasForeignKey(x => x.CarrierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
