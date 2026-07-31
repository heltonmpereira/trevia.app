namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.TrainingPlans;

public class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
{
    public void Configure(EntityTypeBuilder<TrainingSession> b)
    {
        b.ToTable("TrainingSessions");
        b.HasKey(s => s.Id);

        b.HasIndex(s => new { s.TrainingPlanId, s.Order }).IsUnique(true);
        b.HasIndex(s => s.TrainingPlanId);

        b.Property(s => s.Name).HasMaxLength(100).IsRequired();
        b.Property(s => s.Description).HasMaxLength(500);
        b.Property(s => s.CoachNotesInternal).HasMaxLength(2000);
        b.Property(s => s.Focus).HasMaxLength(500);

        b.Property(s => s.Order).IsRequired();

        b.HasOne(s => s.TrainingPlan)
         .WithMany(tp => tp.Sessions)
         .HasForeignKey(s => s.TrainingPlanId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(s => s.Exercises)
         .WithOne(se => se.TrainingSession)
         .HasForeignKey(se => se.TrainingSessionId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
