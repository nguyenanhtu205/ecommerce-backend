namespace SellerService.Infrastructure.Data.Configurations;

public class ShopPaymentSettingsConfiguration : IEntityTypeConfiguration<ShopPaymentSetting>
{
    public void Configure(EntityTypeBuilder<ShopPaymentSetting> builder)
    {
        builder.ToTable("shop_payment_settings");

        builder.HasKey(x => x.ShopId);

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id")
            .ValueGeneratedNever();

        builder.Property(x => x.PayoutCycle)
            .HasColumnName("payout_cycle")
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(PayoutCycle.Weekly)
            .IsRequired();

        builder.HasOne(x => x.Shop)
            .WithOne(x => x.ShopPaymentSetting)
            .HasForeignKey<ShopPaymentSetting>(x => x.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
