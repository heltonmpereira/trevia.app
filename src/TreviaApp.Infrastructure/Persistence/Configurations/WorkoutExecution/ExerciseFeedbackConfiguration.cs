namespace TreviaApp.Infrastructure.Persistence.Configurations.WorkoutExecution;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.WorkoutExecution.Feedback;
using TreviaApp.Shared.Enums;

public class ExerciseFeedbackConfiguration : IEntityTypeConfiguration<ExerciseFeedback>
{
    public void Configure(EntityTypeBuilder<ExerciseFeedback> b)
    {
        b.ToTable("ExerciseFeedbacks");
        b.HasKey(w => w.Id);

        b.Property(w => w.Text).IsRequired().HasMaxLength(4000);
        b.Property(w => w.Tone).IsRequired().HasConversion<string>();
        b.Property(w => w.IsPublic).HasDefaultValue(true);
        b.Property(w => w.StudentResponseText).HasMaxLength(4000);
        b.Property(w => w.IsDeleted).HasDefaultValue(false);

        b.HasIndex(w => new { w.StudentId, w.CreatedAt }).IsDescending(false, true);
        b.HasIndex(w => new { w.CoachId, w.StudentId });
        b.HasIndex(w => w.WorkoutSessionId);
        b.HasIndex(w => w.WorkoutExerciseId);

        b.HasOne(w => w.Coach).WithMany().HasForeignKey(w => w.CoachId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(w => w.Student).WithMany().HasForeignKey(w => w.StudentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(w => w.WorkoutSession).WithMany().HasForeignKey(w => w.WorkoutSessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(w => w.WorkoutExercise).WithMany().HasForeignKey(w => w.WorkoutExerciseId).OnDelete(DeleteBehavior.Cascade);
    }
}
