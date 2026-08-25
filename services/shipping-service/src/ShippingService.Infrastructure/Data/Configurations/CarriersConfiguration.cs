namespace ShippingService.Infrastructure.Data.Configurations;

public class CarriersConfiguration : IEntityTypeConfiguration<Carrier>
{
    public void Configure(EntityTypeBuilder<Carrier> builder)
    {
        builder.ToTable("carriers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasData(
            new Carrier
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Code = "mock",
                Name = "Giao Hàng Thử Nghiệm"
            },
            new Carrier
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Code = "ghn", Name = "Giao Hàng Nhanh"
            },
            new Carrier
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Code = "ghtk", Name = "Giao Hàng Tiết Kiệm"
            }
        );
    }
}
