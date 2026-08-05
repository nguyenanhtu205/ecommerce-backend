namespace OrderService.Infrastructure.Data.Configurations;

public class OrderShippingSnapshotsConfiguration : IEntityTypeConfiguration<OrderShippingSnapshot>
{
    public void Configure(EntityTypeBuilder<OrderShippingSnapshot> builder)
    {
        builder.ToTable("order_shipping_snapshots");

        builder.HasKey(x => x.OrderId);

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .ValueGeneratedNever();

        builder.Property(x => x.CarrierId)
            .HasColumnName("carrier_id")
            .IsRequired();

        builder.Property(x => x.CarrierName)
            .HasColumnName("carrier_name")
            .IsRequired();

        builder.Property(x => x.Fee)
            .HasColumnName("fee")
            .IsRequired();

        builder.Property(x => x.EstimatedDeliveryStart)
            .HasColumnName("estimated_delivery_start")
            .HasColumnType("date");

        builder.Property(x => x.EstimatedDeliveryEnd)
            .HasColumnName("estimated_delivery_end")
            .HasColumnType("date");

        builder.Property(x => x.LateDeliveryCompensation)
            .HasColumnName("late_delivery_compensation");

        builder.HasOne(x => x.Order)
            .WithOne(x => x.OrderShippingSnapshot)
            .HasForeignKey<OrderShippingSnapshot>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
