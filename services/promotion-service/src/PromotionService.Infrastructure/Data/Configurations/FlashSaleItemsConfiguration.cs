namespace PromotionService.Infrastructure.Data.Configurations;

public class FlashSaleItemsConfiguration : IEntityTypeConfiguration<FlashSaleItem>
{
    public void Configure(EntityTypeBuilder<FlashSaleItem> builder)
    {
        builder.ToTable("flash_sale_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.CampaignId)
            .HasColumnName("campaign_id")
            .IsRequired();

        builder.Property(x => x.CombinationId)
            .HasColumnName("combination_id")
            .IsRequired();

        builder.Property(x => x.DiscountedPrice)
            .HasColumnName("discounted_price")
            .IsRequired();

        builder.Property(x => x.QuantityLimit)
            .HasColumnName("quantity_limit");

        builder.Property(x => x.QuantitySold)
            .HasColumnName("quantity_sold")
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasIndex(x => x.CombinationId);

        builder.HasOne(x => x.FlashSaleCampaign)
            .WithMany(x => x.FlashSaleItems)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
