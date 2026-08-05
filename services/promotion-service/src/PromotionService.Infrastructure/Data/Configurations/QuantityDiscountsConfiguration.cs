namespace PromotionService.Infrastructure.Data.Configurations;

public class QuantityDiscountsConfiguration : IEntityTypeConfiguration<QuantityDiscount>
{
    public void Configure(EntityTypeBuilder<QuantityDiscount> builder)
    {
        builder.ToTable("quantity_discounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.MinQuantity)
            .HasColumnName("min_quantity")
            .IsRequired();

        builder.Property(x => x.DiscountPercent)
            .HasColumnName("discount_percent")
            .IsRequired();
    }
}
