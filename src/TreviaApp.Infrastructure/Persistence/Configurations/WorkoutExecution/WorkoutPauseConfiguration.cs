namespace TreviaApp.Infrastructure.Persistence.Configurations.WorkoutExecution;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.WorkoutExecution;

public class WorkoutPauseConfiguration : IEntityTypeConfiguration<WorkoutPause>
{
    public void Configure(EntityTypeBuilder<WorkoutPause> b)
    {
        b.ToTable("WorkoutPauses");

        b.HasKey(w => w.Id);

        b.Property(w => w.StartedAt).IsRequired();

        b.HasIndex(w => w.WorkoutSessionId);
        b.HasIndex(w => new { w.WorkoutSessionId, w.StartedAt });

        b.HasOne(w => w.WorkoutSession)
            .WithMany(w => w.Pauses)
            .HasForeignKey(w => w.WorkoutSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
