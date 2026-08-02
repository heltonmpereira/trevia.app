namespace TreviaApp.Infrastructure.Persistence.Configurations.WorkoutExecution;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Shared.Enums;

public class WorkoutSessionConfiguration : IEntityTypeConfiguration<WorkoutSession>
{
    public void Configure(EntityTypeBuilder<WorkoutSession> b)
    {
        b.ToTable("WorkoutSessions");

        b.HasKey(w => w.Id);

        b.Property(w => w.Name).IsRequired().HasMaxLength(200);

        b.Property(w => w.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(WorkoutStatus.NotStarted);

        b.Property(w => w.OverallRating).HasConversion<string>();
        b.Property(w => w.GeneralNotes).HasMaxLength(2000);

        b.Property(w => w.ActiveTime)
            .HasConversion(
                v => v.HasValue ? (long?)v.Value.TotalSeconds : null,
                v => v.HasValue ? TimeSpan.FromSeconds(v.Value) : null);

        b.HasIndex(w => w.StudentId);
        b.HasIndex(w => w.Status);
        b.HasIndex(w => new { w.StudentId, w.Status });
        b.HasIndex(w => w.TrainingSessionId);
        b.HasIndex(w => w.TrainingPlanId);
        b.HasIndex(w => w.StartedAt);

        b.HasOne(w => w.Student)
            .WithMany()
            .HasForeignKey(w => w.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(w => w.TrainingPlan)
            .WithMany()
            .HasForeignKey(w => w.TrainingPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(w => w.TrainingSession)
            .WithMany()
            .HasForeignKey(w => w.TrainingSessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
