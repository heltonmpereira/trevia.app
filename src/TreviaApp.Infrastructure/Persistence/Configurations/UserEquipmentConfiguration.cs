namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Profiles;

public class UserEquipmentConfiguration : IEntityTypeConfiguration<UserEquipment>
{
    public void Configure(EntityTypeBuilder<UserEquipment> b)
    {
        b.ToTable("UserEquipments");
        b.HasKey(eq => new { eq.ProfileId, eq.Equipment });
        b.Property(eq => eq.Equipment).IsRequired();
        b.Property(eq => eq.AddedAt).IsRequired();
        b.HasOne(eq => eq.Profile).WithMany(p => p.Equipments).HasForeignKey(eq => eq.ProfileId);
    }
}
