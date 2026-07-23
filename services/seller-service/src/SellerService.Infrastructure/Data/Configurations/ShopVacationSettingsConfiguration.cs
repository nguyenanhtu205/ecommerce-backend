namespace SellerService.Infrastructure.Data.Configurations;

public class ShopVacationSettingsConfiguration : IEntityTypeConfiguration<ShopVacationSetting>
{
    public void Configure(EntityTypeBuilder<ShopVacationSetting> builder)
    {
        builder.ToTable("shop_vacation_settings");

        builder.HasKey(x => x.ShopId);

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id")
            .ValueGeneratedNever();

        builder.Property(x => x.IsEnabled)
            .HasColumnName("is_enabled")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("date");

        builder.Property(x => x.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("date");

        builder.Property(x => x.Message)
            .HasColumnName("message")
            .HasColumnType("text");

        builder.HasOne(x => x.Shop)
            .WithOne(x => x.ShopVacationSetting)
            .HasForeignKey<ShopVacationSetting>(x => x.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
