namespace SellerService.Infrastructure.Data.Configurations;

public class ShopChatQuickRepliesConfiguration : IEntityTypeConfiguration<ShopChatQuickReply>
{
    public void Configure(EntityTypeBuilder<ShopChatQuickReply> builder)
    {
        builder.ToTable("shop_chat_quick_replies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.ShopId)
            .HasColumnName("shop_id")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(x => x.ShopId);

        builder.HasOne(x => x.Shop)
            .WithMany(x => x.ShopChatQuickReplies)
            .HasForeignKey(x => x.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
