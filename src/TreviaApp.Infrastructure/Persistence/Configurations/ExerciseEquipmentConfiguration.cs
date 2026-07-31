namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Exercises;

public class ExerciseEquipmentConfiguration : IEntityTypeConfiguration<ExerciseEquipment>
{
    public void Configure(EntityTypeBuilder<ExerciseEquipment> b)
    {
        b.ToTable("ExerciseEquipments");
        b.HasKey(eq => eq.Id);

        b.HasIndex(eq => new { eq.ExerciseId, eq.Equipment }).IsUnique(true);
        b.HasIndex(eq => eq.ExerciseId);
        b.HasIndex(eq => eq.Equipment);

        b.Property(eq => eq.Equipment).IsRequired();
        b.Property(eq => eq.Required).IsRequired();

        b.HasOne(eq => eq.Exercise)
         .WithMany(e => e.Equipments)
         .HasForeignKey(eq => eq.ExerciseId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
