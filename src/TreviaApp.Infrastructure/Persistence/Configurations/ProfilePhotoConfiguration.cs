namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Profiles;

public class ProfilePhotoConfiguration : IEntityTypeConfiguration<ProfilePhoto>
{
    public void Configure(EntityTypeBuilder<ProfilePhoto> b)
    {
        b.ToTable("ProfilePhotos");
        b.HasKey(ph => ph.Id);
        b.HasIndex(ph => ph.ProfileId).IsUnique();
        b.Property(ph => ph.FileId).HasMaxLength(255).IsRequired();
        b.Property(ph => ph.FileName).HasMaxLength(255).IsRequired();
        b.Property(ph => ph.ContentType).HasMaxLength(100).IsRequired();
        b.Property(ph => ph.SizeBytes).IsRequired();
        b.Property(ph => ph.UploadedAt).IsRequired();
    }
}
