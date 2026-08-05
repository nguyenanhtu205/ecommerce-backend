namespace PaymentService.Infrastructure.Data.Configurations;

public class PaymentsConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.BuyerId)
            .HasColumnName("buyer_id")
            .IsRequired();

        builder.Property(x => x.CheckoutBatchId)
            .HasColumnName("checkout_batch_id")
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .IsRequired();

        builder.Property(x => x.Method)
            .HasColumnName("method")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(PaymentStatus.Pending)
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .IsRequired();

        builder.Property(x => x.ProviderTransactionId)
            .HasColumnName("provider_transaction_id");

        builder.Property(x => x.RedirectUrl)
            .HasColumnName("redirect_url");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => x.CheckoutBatchId);

        builder.HasIndex(x => x.BuyerId);

        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique();
    }
}
