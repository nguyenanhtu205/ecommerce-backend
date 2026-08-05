namespace PaymentService.Infrastructure.Data.Configurations;

public class EscrowHoldsConfiguration : IEntityTypeConfiguration<EscrowHold>
{
    public void Configure(EntityTypeBuilder<EscrowHold> builder)
    {
        builder.ToTable("escrow_holds");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id")
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(EscrowStatus.Held)
            .IsRequired();

        builder.Property(x => x.HeldAt)
            .HasColumnName("held_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.ReleaseDueAt)
            .HasColumnName("release_due_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.ReleasedAt)
            .HasColumnName("released_at")
            .HasColumnType("timestamptz");

        builder.HasIndex(x => x.OrderId)
            .IsUnique();

        builder.HasIndex(x => x.ShopId);

        builder.HasIndex(x => new { x.Status, x.ReleaseDueAt });

        builder.HasOne(x => x.Payment)
            .WithMany(x => x.EscrowHolds)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
