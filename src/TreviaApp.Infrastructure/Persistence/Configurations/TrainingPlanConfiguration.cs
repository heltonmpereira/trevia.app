namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.TrainingPlans;

public class TrainingPlanConfiguration : IEntityTypeConfiguration<TrainingPlan>
{
    public void Configure(EntityTypeBuilder<TrainingPlan> b)
    {
        b.ToTable("TrainingPlans");
        b.HasKey(p => p.Id);

        b.HasIndex(p => new { p.IsPublicTemplate, p.Status, p.Visibility, p.SplitType });
        b.HasIndex(p => new { p.CreatedByUserId, p.Status });
        b.HasIndex(p => new { p.AssignedToStudentId, p.Status });
        b.HasIndex(p => new { p.CreatedByUserId, p.Name });
        b.HasIndex(p => p.Name);

        b.Property(p => p.Name).HasMaxLength(200).IsRequired();
        b.Property(p => p.Description).HasMaxLength(1000);
        b.Property(p => p.InstructionsIntro).HasMaxLength(2000);
        b.Property(p => p.NotesForStudent).HasMaxLength(2000);
        b.Property(p => p.Tags).HasMaxLength(500);

        b.Property(p => p.SplitType).IsRequired();
        b.Property(p => p.Status).IsRequired();
        b.Property(p => p.Visibility).IsRequired();
        b.Property(p => p.Version).IsRequired().HasDefaultValue(1);

        b.HasOne(tp => tp.CreatedByUser)
         .WithMany()
         .HasForeignKey(tp => tp.CreatedByUserId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(tp => tp.AssignedToStudent)
         .WithMany()
         .HasForeignKey(tp => tp.AssignedToStudentId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasMany(tp => tp.Sessions)
         .WithOne(s => s.TrainingPlan)
         .HasForeignKey(s => s.TrainingPlanId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
