namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Profiles;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> b)
    {
        b.ToTable("UserProfiles");
        b.HasKey(p => p.Id);
        b.HasIndex(p => p.UserId).IsUnique();
        b.HasIndex(p => new { p.UserId, p.IsDeleted });

        b.Property(p => p.Bio).HasMaxLength(500);
        b.Property(p => p.PreferredUnits).HasMaxLength(20).IsRequired().HasDefaultValue("Metric");

        b.Property(p => p.Goal).IsRequired();
        b.Property(p => p.Experience).IsRequired();
        b.Property(p => p.PreferredEnvironment).IsRequired();
        b.Property(p => p.PrivacyLevel).IsRequired();

        b.HasOne(p => p.User)
         .WithOne()
         .HasForeignKey<UserProfile>(p => p.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(p => p.WeightEntries)
         .WithOne(w => w.Profile)
         .HasForeignKey(w => w.ProfileId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(p => p.Measurements)
         .WithOne(m => m.Profile)
         .HasForeignKey(m => m.ProfileId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(p => p.Photo)
         .WithOne(ph => ph.Profile)
         .HasForeignKey<ProfilePhoto>(ph => ph.ProfileId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasMany(p => p.Equipments)
         .WithOne(eq => eq.Profile)
         .HasForeignKey(eq => eq.ProfileId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
