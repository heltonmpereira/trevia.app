namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Identity;

public class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent>
{
    public void Configure(EntityTypeBuilder<UserConsent> b)
    {
        b.ToTable("UserConsents");
        b.HasKey(c => c.Id);
        b.HasIndex(c => new { c.UserId, c.ConsentType, c.ConsentVersion }).IsUnique();
        b.HasIndex(c => c.UserId);
        b.Property(c => c.ConsentType).IsRequired();
        b.Property(c => c.ConsentVersion).HasMaxLength(20).IsRequired();
        b.Property(c => c.AcceptedAt).IsRequired();
        b.Property(c => c.IpAddress).HasMaxLength(45);
        b.Property(c => c.UserAgent).HasMaxLength(500);
        b.Property(c => c.RevocationReason).HasMaxLength(200);

        b.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
