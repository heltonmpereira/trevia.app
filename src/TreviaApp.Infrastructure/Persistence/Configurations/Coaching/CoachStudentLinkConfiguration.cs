namespace TreviaApp.Infrastructure.Persistence.Configurations.Coaching;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Coaching;

public class CoachStudentLinkConfiguration : IEntityTypeConfiguration<CoachStudentLink>
{
    public void Configure(EntityTypeBuilder<CoachStudentLink> b)
    {
        b.ToTable("CoachStudentLinks");

        b.HasKey(l => l.Id);

        b.Property(l => l.Permissions).IsRequired();

        b.Property(l => l.StartedAt).IsRequired();
        b.Property(l => l.IsActive).IsRequired();

        b.Property(l => l.EndReason)
         .HasConversion<string>();

        b.Property(l => l.EndReasonNotes).HasMaxLength(1000);

        b.HasIndex(l => new { l.CoachId, l.StudentId })
         .HasFilter("[IsActive] = 1 AND [IsDeleted] = 0")
         .IsUnique()
         .HasDatabaseName("IX_CoachStudentLinks_UniqueActivePair");

        b.HasIndex(l => l.CoachId);
        b.HasIndex(l => l.StudentId);
        b.HasIndex(l => l.IsActive);
        b.HasIndex(l => new { l.StudentId, l.IsActive });
        b.HasIndex(l => new { l.CoachId, l.IsActive });

        b.HasOne(l => l.Coach)
         .WithMany()
         .HasForeignKey(l => l.CoachId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(l => l.Student)
         .WithMany()
         .HasForeignKey(l => l.StudentId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
