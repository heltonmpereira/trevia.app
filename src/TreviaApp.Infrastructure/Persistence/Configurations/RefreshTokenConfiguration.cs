namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Infrastructure.Identity;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.HasKey(t => t.Id);
        b.HasIndex(t => t.TokenId).IsUnique();
        b.HasIndex(t => new { t.UserId, t.IsRevoked, t.ExpiresAt });
        b.Property(t => t.TokenId).HasMaxLength(100).IsRequired();
        b.Property(t => t.TokenHash).HasMaxLength(256).IsRequired();
        b.Property(t => t.DeviceInfo).HasMaxLength(256);
        b.Property(t => t.IpAddress).HasMaxLength(45);
        b.Property(t => t.RevocationReason).HasMaxLength(200);
        b.Property(t => t.ReplacedByTokenId).HasMaxLength(100);
        b.Property(t => t.CreatedAt).HasColumnType("timestamptz").IsRequired();
        b.Property(t => t.ExpiresAt).HasColumnType("timestamptz").IsRequired();
        b.Property(t => t.RevokedAt).HasColumnType("timestamptz");
        b.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
