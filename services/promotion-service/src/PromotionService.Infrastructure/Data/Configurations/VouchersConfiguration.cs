namespace PromotionService.Infrastructure.Data.Configurations;

public class VouchersConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("vouchers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .IsRequired();

        builder.Property(x => x.Scope)
            .HasColumnName("scope")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id");

        builder.Property(x => x.DiscountAmount)
            .HasColumnName("discount_amount");

        builder.Property(x => x.DiscountPercent)
            .HasColumnName("discount_percent");

        builder.Property(x => x.MinOrderValue)
            .HasColumnName("min_order_value")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.MaxDiscountAmount)
            .HasColumnName("max_discount_amount");

        builder.Property(x => x.QuantityLimit)
            .HasColumnName("quantity_limit");

        builder.Property(x => x.QuantityUsed)
            .HasColumnName("quantity_used")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.StartsAt)
            .HasColumnName("starts_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.EndsAt)
            .HasColumnName("ends_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();
        
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}
