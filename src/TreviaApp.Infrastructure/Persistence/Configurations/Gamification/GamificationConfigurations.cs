namespace TreviaApp.Infrastructure.Persistence.Configurations.Gamification;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TreviaApp.Domain.Gamification;
using TreviaApp.Shared.Enums;

public class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(EntityTypeBuilder<PointTransaction> b)
    {
        b.ToTable("PointTransactions");

        b.HasKey(p => p.Id);

        b.Property(p => p.Amount).IsRequired();
        b.Property(p => p.Reason)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        b.Property(p => p.ReferenceType).HasMaxLength(100);
        b.Property(p => p.Description).HasMaxLength(500);
        b.Property(p => p.CreatedAt).IsRequired();

        b.Ignore(p => p.IsDeleted);
        b.Ignore(p => p.UpdatedAt);

        b.HasIndex(p => new { p.UserId, p.CreatedAt }).IsDescending(new[] { false, true });
        b.HasIndex(p => new { p.UserId, p.Reason });
        b.HasIndex(p => new { p.ReferenceType, p.ReferenceId });

        b.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserLevelConfiguration : IEntityTypeConfiguration<UserLevel>
{
    public void Configure(EntityTypeBuilder<UserLevel> b)
    {
        b.ToTable("UserLevels");

        b.HasKey(u => u.Id);

        b.Property(u => u.CurrentLevel).IsRequired().HasDefaultValue(1);
        b.Property(u => u.CurrentXp).IsRequired().HasDefaultValue(0L);
        b.Property(u => u.TotalXpEarned).IsRequired().HasDefaultValue(0L);

        b.HasIndex(u => u.UserId).IsUnique();

        b.HasOne(u => u.User)
            .WithOne()
            .HasForeignKey<UserLevel>(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AchievementDefinitionConfiguration : IEntityTypeConfiguration<AchievementDefinition>
{
    public void Configure(EntityTypeBuilder<AchievementDefinition> b)
    {
        b.ToTable("AchievementDefinitions");

        b.HasKey(a => a.Id);

        b.Property(a => a.Code).IsRequired().HasMaxLength(50);
        b.Property(a => a.Name).IsRequired().HasMaxLength(150);
        b.Property(a => a.Description).IsRequired().HasMaxLength(1000);
        b.Property(a => a.Icon).HasMaxLength(500);
        b.Property(a => a.PointsReward).IsRequired().HasDefaultValue(0);
        b.Property(a => a.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);
        b.Property(a => a.CriteriaConfigJson).HasColumnType("jsonb");
        b.Property(a => a.IsActive).IsRequired().HasDefaultValue(true);

        b.HasIndex(a => a.Code).IsUnique();
        b.HasIndex(a => a.Category);
        b.HasIndex(a => a.IsActive);
    }
}

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> b)
    {
        b.ToTable("UserAchievements");

        b.HasKey(u => u.Id);

        b.Property(u => u.Progress).IsRequired().HasDefaultValue(0.0);

        b.HasIndex(u => new { u.UserId, u.AchievementDefinitionId }).IsUnique();
        b.HasIndex(u => u.UserId);
        b.HasIndex(u => u.AchievementDefinitionId);
        b.HasIndex(u => u.UnlockedAt);

        b.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(u => u.AchievementDefinition)
            .WithMany(a => a.UserAchievements)
            .HasForeignKey(u => u.AchievementDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserStreakConfiguration : IEntityTypeConfiguration<UserStreak>
{
    public void Configure(EntityTypeBuilder<UserStreak> b)
    {
        b.ToTable("UserStreaks");

        b.HasKey(u => u.Id);

        b.Property(u => u.DailyCurrent).IsRequired().HasDefaultValue(0);
        b.Property(u => u.DailyLongest).IsRequired().HasDefaultValue(0);
        b.Property(u => u.WeeklyCurrent).IsRequired().HasDefaultValue(0);
        b.Property(u => u.WeeklyLongest).IsRequired().HasDefaultValue(0);

        b.HasIndex(u => u.UserId).IsUnique();

        b.HasOne(u => u.User)
            .WithOne()
            .HasForeignKey<UserStreak>(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DailyMissionDefinitionConfiguration : IEntityTypeConfiguration<DailyMissionDefinition>
{
    public void Configure(EntityTypeBuilder<DailyMissionDefinition> b)
    {
        b.ToTable("DailyMissionDefinitions");

        b.HasKey(m => m.Id);

        b.Property(m => m.Code).IsRequired().HasMaxLength(50);
        b.Property(m => m.Title).IsRequired().HasMaxLength(200);
        b.Property(m => m.Description).IsRequired().HasMaxLength(1000);
        b.Property(m => m.TargetValue).IsRequired();
        b.Property(m => m.Metric)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40);
        b.Property(m => m.PointsReward).IsRequired();
        b.Property(m => m.XpReward).IsRequired();
        b.Property(m => m.IsActive).IsRequired().HasDefaultValue(true);

        b.HasIndex(m => m.Code).IsUnique();
        b.HasIndex(m => m.IsActive);
    }
}

public class WeeklyMissionDefinitionConfiguration : IEntityTypeConfiguration<WeeklyMissionDefinition>
{
    public void Configure(EntityTypeBuilder<WeeklyMissionDefinition> b)
    {
        b.ToTable("WeeklyMissionDefinitions");

        b.HasKey(m => m.Id);

        b.Property(m => m.Code).IsRequired().HasMaxLength(50);
        b.Property(m => m.Title).IsRequired().HasMaxLength(200);
        b.Property(m => m.Description).IsRequired().HasMaxLength(1000);
        b.Property(m => m.TargetValue).IsRequired();
        b.Property(m => m.Metric)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40);
        b.Property(m => m.PointsReward).IsRequired();
        b.Property(m => m.XpReward).IsRequired();
        b.Property(m => m.IsActive).IsRequired().HasDefaultValue(true);

        b.HasIndex(m => m.Code).IsUnique();
        b.HasIndex(m => m.IsActive);
    }
}

public class UserDailyMissionConfiguration : IEntityTypeConfiguration<UserDailyMission>
{
    public void Configure(EntityTypeBuilder<UserDailyMission> b)
    {
        b.ToTable("UserDailyMissions");

        b.HasKey(m => m.Id);

        b.Property(m => m.Date).IsRequired();
        b.Property(m => m.CurrentValue).IsRequired().HasDefaultValue(0);
        b.Property(m => m.IsCompleted).IsRequired().HasDefaultValue(false);

        b.HasIndex(m => new { m.UserId, m.MissionId, m.Date }).IsUnique();
        b.HasIndex(m => m.UserId);
        b.HasIndex(m => m.MissionId);
        b.HasIndex(m => new { m.UserId, m.Date });
        b.HasIndex(m => m.IsCompleted);

        b.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(m => m.Mission)
            .WithMany(md => md.UserDailyMissions)
            .HasForeignKey(m => m.MissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserWeeklyMissionConfiguration : IEntityTypeConfiguration<UserWeeklyMission>
{
    public void Configure(EntityTypeBuilder<UserWeeklyMission> b)
    {
        b.ToTable("UserWeeklyMissions");

        b.HasKey(m => m.Id);

        b.Property(m => m.WeekStart).IsRequired();
        b.Property(m => m.CurrentValue).IsRequired().HasDefaultValue(0);
        b.Property(m => m.IsCompleted).IsRequired().HasDefaultValue(false);

        b.HasIndex(m => new { m.UserId, m.MissionId, m.WeekStart }).IsUnique();
        b.HasIndex(m => m.UserId);
        b.HasIndex(m => m.MissionId);
        b.HasIndex(m => new { m.UserId, m.WeekStart });
        b.HasIndex(m => m.IsCompleted);

        b.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(m => m.Mission)
            .WithMany(md => md.UserWeeklyMissions)
            .HasForeignKey(m => m.MissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
