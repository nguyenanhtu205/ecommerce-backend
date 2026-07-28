namespace InventoryService.Infrastructure.Data.Configurations;

public class ProductVariantCombinationsConfiguration : IEntityTypeConfiguration<ProductVariantCombination>
{
    public void Configure(EntityTypeBuilder<ProductVariantCombination> builder)
    {
        builder.ToTable("product_variant_combinations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id")
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasColumnName("sku")
            .HasMaxLength(255);

        builder.Property(x => x.Price)
            .HasColumnName("price")
            .IsRequired();

        builder.Property(x => x.Stock)
            .HasColumnName("stock")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.ReservedStock)
            .HasColumnName("reserved_stock")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .HasDefaultValue(0)
            .IsRequired()
            .IsConcurrencyToken();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => x.ProductId);
    }
}
