namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Profiles;

public class PhysicalMeasurementConfiguration : IEntityTypeConfiguration<PhysicalMeasurement>
{
    public void Configure(EntityTypeBuilder<PhysicalMeasurement> b)
    {
        b.ToTable("PhysicalMeasurements");
        b.HasKey(m => m.Id);
        b.HasIndex(m => new { m.ProfileId, m.MeasuredAt }).IsDescending(false, true);
        b.Property(m => m.MeasuredAt).IsRequired();
        b.Property(m => m.Note).HasMaxLength(500);
    }
}
