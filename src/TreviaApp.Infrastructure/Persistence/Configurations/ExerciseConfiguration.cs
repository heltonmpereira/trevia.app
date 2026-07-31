namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Exercises;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> b)
    {
        b.ToTable("Exercises");
        b.HasKey(e => e.Id);

        b.HasIndex(e => e.Name);
        b.HasIndex(e => e.Slug).IsUnique(false);
        b.HasIndex(e => new { e.CreatedByUserId, e.Slug }).IsUnique(true);
        b.HasIndex(e => new { e.Status, e.Visibility, e.Environment, e.Modality, e.DifficultyLevel });
        b.HasIndex(e => new { e.CreatedByUserId, e.Status });

        b.Property(e => e.Name).HasMaxLength(200).IsRequired();
        b.Property(e => e.Slug).HasMaxLength(250).IsRequired();
        b.Property(e => e.ShortDescription).HasMaxLength(500);
        b.Property(e => e.Instructions).HasMaxLength(4000).IsRequired();
        b.Property(e => e.Tips).HasMaxLength(2000);
        b.Property(e => e.Tags).HasMaxLength(500);
        b.Property(e => e.RejectReason).HasMaxLength(1000);

        b.Property(e => e.Environment).IsRequired();
        b.Property(e => e.Modality).IsRequired();
        b.Property(e => e.DifficultyLevel).IsRequired();
        b.Property(e => e.MeasurementType).IsRequired();
        b.Property(e => e.Visibility).IsRequired();
        b.Property(e => e.Status).IsRequired();

        b.HasOne(e => e.CreatedByUser)
         .WithMany()
         .HasForeignKey(e => e.CreatedByUserId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(e => e.ApprovedByUser)
         .WithMany()
         .HasForeignKey(e => e.ApprovedByUserId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(e => e.RejectedByUser)
         .WithMany()
         .HasForeignKey(e => e.RejectedByUserId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(e => e.Muscles)
         .WithOne(em => em.Exercise)
         .HasForeignKey(em => em.ExerciseId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(e => e.Equipments)
         .WithOne(eq => eq.Exercise)
         .HasForeignKey(eq => eq.ExerciseId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(e => e.Medias)
         .WithOne(m => m.Exercise)
         .HasForeignKey(m => m.ExerciseId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
