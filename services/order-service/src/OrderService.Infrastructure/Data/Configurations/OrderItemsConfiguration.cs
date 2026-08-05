namespace OrderService.Infrastructure.Data.Configurations;

public class OrderItemsConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.CombinationId)
            .HasColumnName("combination_id")
            .IsRequired();

        builder.Property(x => x.ProductName)
            .HasColumnName("product_name")
            .IsRequired();

        builder.Property(x => x.ThumbnailUrl)
            .HasColumnName("thumbnail_url")
            .IsRequired();

        builder.Property(x => x.Variation)
            .HasColumnName("variation");

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnName("price")
            .IsRequired();

        builder.Property(x => x.OriginalPrice)
            .HasColumnName("original_price");

        builder.HasIndex(x => x.OrderId);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
