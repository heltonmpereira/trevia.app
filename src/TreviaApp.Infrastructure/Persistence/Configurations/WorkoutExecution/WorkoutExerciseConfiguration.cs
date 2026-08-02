namespace TreviaApp.Infrastructure.Persistence.Configurations.WorkoutExecution;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.WorkoutExecution;

public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutExercise> b)
    {
        b.ToTable("WorkoutExercises");

        b.HasKey(w => w.Id);

        b.Property(w => w.SkipReason).HasMaxLength(500);
        b.Property(w => w.Notes).HasMaxLength(1000);

        b.HasIndex(w => w.WorkoutSessionId);
        b.HasIndex(w => new { w.WorkoutSessionId, w.Order });
        b.HasIndex(w => w.SessionExerciseId);
        b.HasIndex(w => w.ExerciseId);

        b.HasOne(w => w.WorkoutSession)
            .WithMany(w => w.Exercises)
            .HasForeignKey(w => w.WorkoutSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(w => w.SessionExercise)
            .WithMany()
            .HasForeignKey(w => w.SessionExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(w => w.Exercise)
            .WithMany()
            .HasForeignKey(w => w.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
