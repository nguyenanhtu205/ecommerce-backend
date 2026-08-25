namespace ShippingService.Infrastructure.Data.Configurations;

public class PickupPointsConfiguration : IEntityTypeConfiguration<PickupPoint>
{
    public void Configure(EntityTypeBuilder<PickupPoint> builder)
    {
        builder.ToTable("pickup_points");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.CarrierId)
            .HasColumnName("carrier_id")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Address)
            .HasColumnName("address")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasOne(x => x.Carrier)
            .WithMany(x => x.PickupPoints)
            .HasForeignKey(x => x.CarrierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new PickupPoint
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                CarrierId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Điểm lấy hàng ở Hà Nội",
                Address = "123 Nguyễn Trãi, Thanh Xuân, Hà Nội"
            },
            new PickupPoint
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                CarrierId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Điểm lấy hàng ở Hồ Chí Minh",
                Address = "456 Nguyễn Thị Minh Khai, Quận 3, TP. Hồ Chí Minh"
            },
            new PickupPoint
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                CarrierId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Điểm lấy hàng ở Đà Nẵng",
                Address = "789 Nguyễn Văn Linh, Hải Châu, Đà Nẵng"
            }
        );
    }
}
