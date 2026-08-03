namespace TreviaApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Identity;

public class ProcessedClientRequestConfiguration : IEntityTypeConfiguration<ProcessedClientRequest>
{
    public void Configure(EntityTypeBuilder<ProcessedClientRequest> builder)
    {
        builder.HasKey(p => p.RequestId);

        builder.Property(p => p.OperationType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ResponsePayload)
            .HasColumnType("jsonb");

        builder.HasIndex(p => new { p.UserId, p.RequestId })
            .IsUnique();

        builder.HasIndex(p => p.UserId);

        builder.HasIndex(p => p.ProcessedAt);

        builder.HasOne<AppUser>(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
