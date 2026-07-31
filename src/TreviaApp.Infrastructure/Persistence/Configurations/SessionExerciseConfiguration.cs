namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.TrainingPlans;

public class SessionExerciseConfiguration : IEntityTypeConfiguration<SessionExercise>
{
    public void Configure(EntityTypeBuilder<SessionExercise> b)
    {
        b.ToTable("SessionExercises");
        b.HasKey(se => se.Id);

        b.HasIndex(se => new { se.TrainingSessionId, se.Order }).IsUnique(true);
        b.HasIndex(se => se.ExerciseId);
        b.HasIndex(se => se.TrainingSessionId);

        b.Property(se => se.Order).IsRequired();
        b.Property(se => se.NotesForStudent).HasMaxLength(1000);
        b.Property(se => se.NotesForCoach).HasMaxLength(1000);

        b.HasOne(se => se.TrainingSession)
         .WithMany(s => s.Exercises)
         .HasForeignKey(se => se.TrainingSessionId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(se => se.Exercise)
         .WithMany()
         .HasForeignKey(se => se.ExerciseId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(se => se.Prescriptions)
         .WithOne(p => p.SessionExercise)
         .HasForeignKey(p => p.SessionExerciseId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
