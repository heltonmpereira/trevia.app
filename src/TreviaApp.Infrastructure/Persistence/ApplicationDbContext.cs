namespace TreviaApp.Infrastructure.Persistence;

using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Domain.Coaching;
using TreviaApp.Domain.Exercises;
using TreviaApp.Domain.Identity;
using TreviaApp.Domain.Interfaces;
using TreviaApp.Domain.Notifications;
using TreviaApp.Domain.Profiles;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Domain.WorkoutExecution.Feedback;
using TreviaApp.Infrastructure.Identity;

public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<WeightEntry> WeightEntries => Set<WeightEntry>();
    public DbSet<PhysicalMeasurement> PhysicalMeasurements => Set<PhysicalMeasurement>();
    public DbSet<ProfilePhoto> ProfilePhotos => Set<ProfilePhoto>();
    public DbSet<UserEquipment> UserEquipments => Set<UserEquipment>();
    public DbSet<UserConsent> UserConsents => Set<UserConsent>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseMuscle> ExerciseMuscles => Set<ExerciseMuscle>();
    public DbSet<ExerciseEquipment> ExerciseEquipments => Set<ExerciseEquipment>();
    public DbSet<ExerciseMedia> ExerciseMedias => Set<ExerciseMedia>();
    public DbSet<TrainingPlan> TrainingPlans => Set<TrainingPlan>();
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<SessionExercise> SessionExercises => Set<SessionExercise>();
    public DbSet<SetPrescription> SetPrescriptions => Set<SetPrescription>();
    public DbSet<CoachStudentRequest> CoachStudentRequests => Set<CoachStudentRequest>();
    public DbSet<CoachStudentLink> CoachStudentLinks => Set<CoachStudentLink>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<WorkoutSet> WorkoutSets => Set<WorkoutSet>();
    public DbSet<WorkoutPause> WorkoutPauses => Set<WorkoutPause>();
    public DbSet<WorkoutFeedback> WorkoutFeedbacks => Set<WorkoutFeedback>();
    public DbSet<ExerciseFeedback> ExerciseFeedbacks => Set<ExerciseFeedback>();
    public DbSet<SetFeedback> SetFeedbacks => Set<SetFeedback>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries();
        foreach (var entry in entries)
        {
            if (entry.Entity is AppUser user)
            {
                if (entry.State == EntityState.Added) user.CreatedAt = DateTimeOffset.UtcNow;
                if (entry.State == EntityState.Modified) user.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .LogTo(message => Debug.WriteLine(message))
            .EnableDetailedErrors();
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
#endif


        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        ConfigureDefaultDecimalPrecision(builder);

        builder.Entity<UserProfile>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<Exercise>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<TrainingPlan>().HasQueryFilter(tp => !tp.IsDeleted);
        builder.Entity<CoachStudentRequest>().HasQueryFilter(r => !r.IsDeleted);
        builder.Entity<CoachStudentLink>().HasQueryFilter(l => !l.IsDeleted);
        builder.Entity<WorkoutSession>().HasQueryFilter(w => !w.IsDeleted);
        builder.Entity<WorkoutExercise>().HasQueryFilter(w => !w.IsDeleted);
        builder.Entity<WorkoutSet>().HasQueryFilter(w => !w.IsDeleted);
        builder.Entity<WorkoutPause>().HasQueryFilter(w => !w.IsDeleted);
        builder.Entity<WorkoutFeedback>().HasQueryFilter(w => !w.IsDeleted);
        builder.Entity<ExerciseFeedback>().HasQueryFilter(w => !w.IsDeleted);
        builder.Entity<SetFeedback>().HasQueryFilter(w => !w.IsDeleted);
        builder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);
    }

    private static void ConfigureDefaultDecimalPrecision(ModelBuilder builder)
    {
        foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(4);
        }
    }
}
