namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Identity;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        b.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        b.Property(u => u.DisplayName).HasMaxLength(100);
        b.Property(u => u.CreatedAt).HasColumnType("timestamptz").IsRequired();
        b.Property(u => u.UpdatedAt).HasColumnType("timestamptz");
        b.Property(u => u.LastActiveAt).HasColumnType("timestamptz");
        b.Property(u => u.IsDeleted).HasDefaultValue(false);
        b.HasIndex(u => u.IsDeleted);
    }
}
