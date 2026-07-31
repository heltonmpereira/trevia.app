namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Profiles;

public class WeightEntryConfiguration : IEntityTypeConfiguration<WeightEntry>
{
    public void Configure(EntityTypeBuilder<WeightEntry> b)
    {
        b.ToTable("WeightEntries");
        b.HasKey(w => w.Id);
        b.HasIndex(w => new { w.ProfileId, w.MeasuredAt }).IsDescending(false, true);
        b.Property(w => w.WeightKg).IsRequired();
        b.Property(w => w.MeasuredAt).IsRequired();
        b.Property(w => w.Note).HasMaxLength(200);
    }
}
