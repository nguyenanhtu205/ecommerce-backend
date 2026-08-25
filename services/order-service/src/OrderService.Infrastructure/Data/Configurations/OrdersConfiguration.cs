namespace OrderService.Infrastructure.Data.Configurations;

public class OrdersConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.BuyerId)
            .HasColumnName("buyer_id")
            .IsRequired();

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id")
            .IsRequired();

        builder.Property(x => x.ShopName)
            .HasColumnName("shop_name")
            .IsRequired();

        builder.Property(x => x.CheckoutBatchId)
            .HasColumnName("checkout_batch_id")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.MerchandiseSubtotal)
            .HasColumnName("merchandise_subtotal")
            .IsRequired();

        builder.Property(x => x.ShippingFee)
            .HasColumnName("shipping_fee")
            .IsRequired();

        builder.Property(x => x.VoucherDiscount)
            .HasColumnName("voucher_discount")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.XuDiscount)
            .HasColumnName("xu_discount")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.TotalPayment)
            .HasColumnName("total_payment")
            .IsRequired();

        builder.Property(x => x.ShippingAddressSnapshot)
            .HasColumnName("shipping_address_snapshot")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => x.BuyerId);

        builder.HasIndex(x => x.ShopId);

        builder.HasIndex(x => x.CheckoutBatchId);

        builder.HasIndex(x => new { x.BuyerId, x.Status });
    }
}
