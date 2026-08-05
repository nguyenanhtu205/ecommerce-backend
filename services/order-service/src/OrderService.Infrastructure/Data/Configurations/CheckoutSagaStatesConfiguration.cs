namespace OrderService.Infrastructure.Data.Configurations;

public class CheckoutSagaStatesConfiguration : IEntityTypeConfiguration<CheckoutSagaState>
{
    public void Configure(EntityTypeBuilder<CheckoutSagaState> builder)
    {
        builder.ToTable("checkout_saga_states");

        builder.HasKey(x => x.CorrelationId);

        builder.Property(x => x.CorrelationId)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.CurrentState)
            .HasColumnName("current_state")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.BuyerId)
            .HasColumnName("buyer_id")
            .IsRequired();

        builder.Property(x => x.OrderIds)
            .HasColumnName("order_ids")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>())
            .Metadata.SetValueComparer(new ValueComparer<List<Guid>>(
                (a, b) => (a ?? new List<Guid>()).SequenceEqual(b ?? new List<Guid>()),
                v => v.Aggregate(0, (hash, g) => HashCode.Combine(hash, g.GetHashCode())),
                v => v.ToList()));

        builder.Property(x => x.ReservedOrderIds)
            .HasColumnName("reserved_order_ids")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>())
            .Metadata.SetValueComparer(new ValueComparer<List<Guid>>(
                (a, b) => (a ?? new List<Guid>()).SequenceEqual(b ?? new List<Guid>()),
                v => v.Aggregate(0, (hash, g) => HashCode.Combine(hash, g.GetHashCode())),
                v => v.ToList()));

        builder.Property(x => x.TotalAmount)
            .HasColumnName("total_amount")
            .IsRequired();

        builder.Property(x => x.OrderShares)
            .HasColumnName("order_shares")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<OrderPaymentShare>>(v, (JsonSerializerOptions?)null) ??
                     new List<OrderPaymentShare>())
            .Metadata.SetValueComparer(new ValueComparer<List<OrderPaymentShare>>(
                (a, b) => (a ?? new List<OrderPaymentShare>()).SequenceEqual(b ?? new List<OrderPaymentShare>()),
                v => v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
                v => v.ToList()));

        builder.Property(x => x.PaymentMethod)
            .HasColumnName("payment_method")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.PlatformVoucherCode)
            .HasColumnName("platform_voucher_code")
            .HasMaxLength(64);

        builder.Property(x => x.ShopVouchers)
            .HasColumnName("shop_vouchers")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<ShopVoucherRedemption>>(v, (JsonSerializerOptions?)null) ??
                     new List<ShopVoucherRedemption>())
            .Metadata.SetValueComparer(new ValueComparer<List<ShopVoucherRedemption>>(
                (a, b) =>
                    (a ?? new List<ShopVoucherRedemption>()).SequenceEqual(b ?? new List<ShopVoucherRedemption>()),
                v => v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
                v => v.ToList()));

        builder.Property(x => x.VoucherRedeemed)
            .HasColumnName("voucher_redeemed")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.RedirectUrl)
            .HasColumnName("redirect_url")
            .HasMaxLength(2048);

        builder.Property(x => x.FailReason)
            .HasColumnName("fail_reason")
            .HasColumnType("text");

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(x => x.BuyerId);
    }
}
