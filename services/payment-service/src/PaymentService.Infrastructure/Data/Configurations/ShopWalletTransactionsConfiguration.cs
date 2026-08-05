namespace PaymentService.Infrastructure.Data.Configurations;

public class ShopWalletTransactionsConfiguration : IEntityTypeConfiguration<ShopWalletTransaction>
{
    public void Configure(EntityTypeBuilder<ShopWalletTransaction> builder)
    {
        builder.ToTable("shop_wallet_transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id")
            .IsRequired();

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired(false);

        builder.Property(x => x.EscrowHoldId)
            .HasColumnName("escrow_hold_id")
            .IsRequired(false);

        builder.Property(x => x.RefundId)
            .HasColumnName("refund_id")
            .IsRequired(false);

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .IsRequired();

        builder.Property(x => x.AvailableBalanceAfter)
            .HasColumnName("available_balance_after")
            .IsRequired();

        builder.Property(x => x.DebtBalanceAfter)
            .HasColumnName("debt_balance_after")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => x.ShopId);

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => new { x.ShopId, x.CreatedAt });

        builder.HasOne(x => x.ShopWallet)
            .WithMany(x => x.ShopWalletTransactions)
            .HasForeignKey(x => x.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EscrowHold)
            .WithMany()
            .HasForeignKey(x => x.EscrowHoldId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Refund)
            .WithMany()
            .HasForeignKey(x => x.RefundId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
