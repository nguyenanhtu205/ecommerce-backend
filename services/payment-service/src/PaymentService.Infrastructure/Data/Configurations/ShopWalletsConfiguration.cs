namespace PaymentService.Infrastructure.Data.Configurations;

public class ShopWalletsConfiguration : IEntityTypeConfiguration<ShopWallet>
{
    public void Configure(EntityTypeBuilder<ShopWallet> builder)
    {
        builder.ToTable("shop_wallets");

        builder.HasKey(x => x.ShopId);

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id")
            .ValueGeneratedNever();

        builder.Property(x => x.AvailableBalance)
            .HasColumnName("available_balance")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.PendingBalance)
            .HasColumnName("pending_balance")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.DebtBalance)
            .HasColumnName("debt_balance")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}
