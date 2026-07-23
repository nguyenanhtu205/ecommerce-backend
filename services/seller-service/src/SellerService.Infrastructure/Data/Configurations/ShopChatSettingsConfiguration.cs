namespace SellerService.Infrastructure.Data.Configurations;

public class ShopChatSettingsConfiguration : IEntityTypeConfiguration<ShopChatSetting>
{
    public void Configure(EntityTypeBuilder<ShopChatSetting> builder)
    {
        builder.ToTable("shop_chat_settings");

        builder.HasKey(x => x.ShopId);

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id")
            .ValueGeneratedNever();

        builder.Property(x => x.AutoReplyEnabled)
            .HasColumnName("auto_reply_enabled")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.AutoReplyMessage)
            .HasColumnName("auto_reply_message")
            .HasColumnType("text");

        builder.Property(x => x.AwayModeEnabled)
            .HasColumnName("away_mode_enabled")
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasOne(x => x.Shop)
            .WithOne(x => x.ShopChatSetting)
            .HasForeignKey<ShopChatSetting>(x => x.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
