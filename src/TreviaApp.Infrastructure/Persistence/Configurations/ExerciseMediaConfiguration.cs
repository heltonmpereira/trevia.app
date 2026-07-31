namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Exercises;

public class ExerciseMediaConfiguration : IEntityTypeConfiguration<ExerciseMedia>
{
    public void Configure(EntityTypeBuilder<ExerciseMedia> b)
    {
        b.ToTable("ExerciseMedias");
        b.HasKey(m => m.Id);

        b.HasIndex(m => m.ExerciseId);
        b.HasIndex(m => new { m.ExerciseId, m.Order });

        b.Property(m => m.FileId).HasMaxLength(255).IsRequired();
        b.Property(m => m.FileName).HasMaxLength(255).IsRequired();
        b.Property(m => m.Caption).HasMaxLength(255);

        b.Property(m => m.MediaType).IsRequired();
        b.Property(m => m.SizeBytes).IsRequired();

        b.HasOne(m => m.Exercise)
         .WithMany(e => e.Medias)
         .HasForeignKey(m => m.ExerciseId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
