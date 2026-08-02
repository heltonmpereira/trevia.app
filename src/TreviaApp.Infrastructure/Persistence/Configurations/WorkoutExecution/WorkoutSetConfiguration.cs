namespace TreviaApp.Infrastructure.Persistence.Configurations.WorkoutExecution;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Shared.Enums;

public class WorkoutSetConfiguration : IEntityTypeConfiguration<WorkoutSet>
{
    public void Configure(EntityTypeBuilder<WorkoutSet> b)
    {
        b.ToTable("WorkoutSets");

        b.HasKey(w => w.Id);

        b.Property(w => w.TargetLoadUnit).HasConversion<string>().HasDefaultValue(PrescriptionLoadUnit.Kilograms);
        b.Property(w => w.ActualLoadUnit).HasConversion<string>().HasDefaultValue(PrescriptionLoadUnit.Kilograms);
        b.Property(w => w.DifficultyRating).HasConversion<string>();
        b.Property(w => w.Notes).HasMaxLength(500);

        b.Property(w => w.TargetRestSeconds)
            .HasConversion(
                v => v.HasValue ? (long?)v.Value.TotalSeconds : null,
                v => v.HasValue ? TimeSpan.FromSeconds(v.Value) : null);

        b.Property(w => w.ActualDuration)
            .HasConversion(
                v => v.HasValue ? (long?)v.Value.TotalSeconds : null,
                v => v.HasValue ? TimeSpan.FromSeconds(v.Value) : null);

        b.Property(w => w.TargetLoadValue).HasPrecision(18, 4);
        b.Property(w => w.ActualLoadValue).HasPrecision(18, 4);
        b.Property(w => w.DistanceKm).HasPrecision(18, 4);
        b.Property(w => w.SpeedKmh).HasPrecision(18, 4);
        b.Property(w => w.InclinePercent).HasPrecision(18, 4);

        b.HasIndex(w => w.WorkoutExerciseId);
        b.HasIndex(w => new { w.WorkoutExerciseId, w.SetNumber });
        b.HasIndex(w => w.SetPrescriptionId);

        b.HasOne(w => w.WorkoutExercise)
            .WithMany(w => w.Sets)
            .HasForeignKey(w => w.WorkoutExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(w => w.SetPrescription)
            .WithMany()
            .HasForeignKey(w => w.SetPrescriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
