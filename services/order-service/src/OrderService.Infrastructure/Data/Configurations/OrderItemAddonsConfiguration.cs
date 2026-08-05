namespace OrderService.Infrastructure.Data.Configurations;

public class OrderItemAddonsConfiguration : IEntityTypeConfiguration<OrderItemAddon>
{
    public void Configure(EntityTypeBuilder<OrderItemAddon> builder)
    {
        builder.ToTable("order_item_addons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.OrderItemId)
            .HasColumnName("order_item_id")
            .IsRequired();

        builder.Property(x => x.Label)
            .HasColumnName("label")
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnName("price")
            .IsRequired();

        builder.HasOne(x => x.OrderItem)
            .WithMany(x => x.OrderItemAddons)
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
