namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Notifications;
using TreviaApp.Shared.Enums;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("Notifications");
        b.HasKey(n => n.Id);

        b.Property(n => n.Type).IsRequired().HasConversion<string>();
        b.Property(n => n.Title).IsRequired().HasMaxLength(200);
        b.Property(n => n.Message).IsRequired().HasMaxLength(1000);
        b.Property(n => n.ReferenceType).HasConversion<string>();
        b.Property(n => n.IsRead).HasDefaultValue(false);
        b.Property(n => n.IsDeleted).HasDefaultValue(false);

        // Critical index for unread badge + paginated list by user/date
        b.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt })
         .IsDescending(false, false, true);

        b.HasIndex(n => new { n.UserId, n.CreatedAt }).IsDescending(false, true);

        b.HasOne(n => n.User)
         .WithMany()
         .HasForeignKey(n => n.UserId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
