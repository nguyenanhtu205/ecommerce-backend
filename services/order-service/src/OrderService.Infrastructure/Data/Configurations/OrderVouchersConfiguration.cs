namespace OrderService.Infrastructure.Data.Configurations;

public class OrderVouchersConfiguration : IEntityTypeConfiguration<OrderVoucher>
{
    public void Configure(EntityTypeBuilder<OrderVoucher> builder)
    {
        builder.ToTable("order_vouchers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.VoucherCode)
            .HasColumnName("voucher_code");

        builder.Property(x => x.DiscountAmount)
            .HasColumnName("discount_amount")
            .IsRequired();

        builder.Property(x => x.Scope)
            .HasColumnName("scope")
            .IsRequired();


        builder.HasOne(x => x.Order)
            .WithMany(x => x.OrderVouchers)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
