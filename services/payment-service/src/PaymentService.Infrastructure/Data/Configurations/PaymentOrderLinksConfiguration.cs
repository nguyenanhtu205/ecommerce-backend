namespace PaymentService.Infrastructure.Data.Configurations;

public class PaymentOrderLinksConfiguration : IEntityTypeConfiguration<PaymentOrderLink>
{
    public void Configure(EntityTypeBuilder<PaymentOrderLink> builder)
    {
        builder.ToTable("payment_order_links");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .IsRequired();

        builder.HasIndex(x => x.PaymentId);

        builder.HasIndex(x => x.OrderId)
            .IsUnique();

        builder.HasOne(x => x.Payment)
            .WithMany(x => x.PaymentOrderLinks)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
