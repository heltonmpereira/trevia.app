namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Enums;

public class SetPrescriptionConfiguration : IEntityTypeConfiguration<SetPrescription>
{
    public void Configure(EntityTypeBuilder<SetPrescription> b)
    {
        b.ToTable("SetPrescriptions");
        b.HasKey(p => p.Id);

        b.HasIndex(p => new { p.SessionExerciseId, p.SetNumber }).IsUnique(true);
        b.HasIndex(p => p.SessionExerciseId);

        b.Property(p => p.SetNumber).IsRequired();
        b.Property(p => p.LoadUnit).IsRequired().HasDefaultValue(PrescriptionLoadUnit.Kilograms);
        b.Property(p => p.TechniqueApplied).IsRequired().HasDefaultValue(SetTechnique.Standard);
        b.Property(p => p.NotesProfessor).HasMaxLength(500);
        b.Property(p => p.TempoNotation).HasMaxLength(10);

        b.HasOne(p => p.SessionExercise)
         .WithMany(se => se.Prescriptions)
         .HasForeignKey(p => p.SessionExerciseId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
