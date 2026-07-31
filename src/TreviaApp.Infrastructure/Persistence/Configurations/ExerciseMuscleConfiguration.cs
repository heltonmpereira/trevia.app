namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Exercises;

public class ExerciseMuscleConfiguration : IEntityTypeConfiguration<ExerciseMuscle>
{
    public void Configure(EntityTypeBuilder<ExerciseMuscle> b)
    {
        b.ToTable("ExerciseMuscles");
        b.HasKey(em => em.Id);

        b.HasIndex(em => new { em.ExerciseId, em.Muscle }).IsUnique(true);
        b.HasIndex(em => em.ExerciseId);
        b.HasIndex(em => em.Muscle);

        b.Property(em => em.ActivationPercent).HasPrecision(5, 2);

        b.Property(em => em.Muscle).IsRequired();
        b.Property(em => em.MuscleRole).IsRequired();

        b.HasOne(m => m.Exercise)
         .WithMany(e => e.Muscles)
         .HasForeignKey(m => m.ExerciseId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
