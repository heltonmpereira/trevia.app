namespace TreviaApp.Infrastructure.Persistence.Configurations.Coaching;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Coaching;
using TreviaApp.Shared.Enums;

public class CoachStudentRequestConfiguration : IEntityTypeConfiguration<CoachStudentRequest>
{
    public void Configure(EntityTypeBuilder<CoachStudentRequest> b)
    {
        b.ToTable("CoachStudentRequests");

        b.HasKey(r => r.Id);

        b.Property(r => r.Message).HasMaxLength(500);
        b.Property(r => r.CoachNotesInternal).HasMaxLength(1000);
        b.Property(r => r.RejectionReason).HasMaxLength(500);

        b.Property(r => r.Direction).IsRequired()
         .HasConversion<string>();

        b.Property(r => r.Status).IsRequired()
         .HasConversion<string>()
         .HasDefaultValue(CoachRequestStatus.Pending);

        b.Property(r => r.GrantedPermissionsOnAccept).IsRequired();

        b.Property(r => r.ExpiresAt).IsRequired();

        b.HasIndex(r => new { r.CoachId, r.StudentId, r.Status })
         .HasFilter("\"Status\" = 'Pending'")
         .IsUnique()
         .HasDatabaseName("IX_CoachStudentRequests_UniquePendingPair");

        b.HasIndex(r => r.CoachId);
        b.HasIndex(r => r.StudentId);
        b.HasIndex(r => r.Status);
        b.HasIndex(r => r.ExpiresAt);

        b.HasOne(r => r.Coach)
         .WithMany()
         .HasForeignKey(r => r.CoachId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(r => r.Student)
         .WithMany()
         .HasForeignKey(r => r.StudentId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
