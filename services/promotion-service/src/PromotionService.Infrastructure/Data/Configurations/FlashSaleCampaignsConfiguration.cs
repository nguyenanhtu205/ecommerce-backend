namespace PromotionService.Infrastructure.Data.Configurations;

public class FlashSaleCampaignsConfiguration : IEntityTypeConfiguration<FlashSaleCampaign>
{
    public void Configure(EntityTypeBuilder<FlashSaleCampaign> builder)
    {
        builder.ToTable("flash_sale_campaigns");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(CampaignStatus.Scheduled)
            .IsRequired();

        builder.Property(x => x.StartsAt)
            .HasColumnName("starts_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.EndsAt)
            .HasColumnName("ends_at")
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}
