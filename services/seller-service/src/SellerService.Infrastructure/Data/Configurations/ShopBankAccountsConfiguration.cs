namespace SellerService.Infrastructure.Data.Configurations;

public class ShopBankAccountsConfiguration : IEntityTypeConfiguration<ShopBankAccount>
{
    public void Configure(EntityTypeBuilder<ShopBankAccount> builder)
    {
        builder.ToTable("shop_bank_accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id")
            .IsRequired();

        builder.Property(x => x.BankName)
            .HasColumnName("bank_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.AccountNumber)
            .HasColumnName("account_number")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AccountHolder)
            .HasColumnName("account_holder")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.IsVerified)
            .HasColumnName("is_verified")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => x.ShopId);

        builder.HasOne(x => x.Shop)
            .WithMany(x => x.ShopBankAccounts)
            .HasForeignKey(x => x.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
