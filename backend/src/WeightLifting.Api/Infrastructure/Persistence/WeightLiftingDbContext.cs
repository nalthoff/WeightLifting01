using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.Infrastructure.Persistence;

public sealed class WeightLiftingDbContext(DbContextOptions<WeightLiftingDbContext> options)
    : DbContext(options)
{
    public DbSet<LiftEntity> Lifts => Set<LiftEntity>();
    public DbSet<WorkoutEntity> Workouts => Set<WorkoutEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LiftEntity>(entity =>
        {
            entity.ToTable("Lifts");

            entity.HasKey(lift => lift.Id);

            entity.Property(lift => lift.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(lift => lift.NameNormalized)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(lift => lift.NameNormalized)
                .IsUnique();

            entity.Property(lift => lift.IsActive)
                .HasDefaultValue(true);

            entity.Property(lift => lift.CreatedAtUtc)
                .IsRequired();
        });

        modelBuilder.Entity<WorkoutEntity>(entity =>
        {
            entity.ToTable("Workouts");

            entity.HasKey(workout => workout.Id);

            entity.Property(workout => workout.UserId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(workout => workout.Status)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(workout => workout.Label)
                .HasMaxLength(Workout.MaxLabelLength);

            entity.Property(workout => workout.StartedAtUtc)
                .IsRequired();

            entity.Property(workout => workout.CreatedAtUtc)
                .IsRequired();

            entity.Property(workout => workout.UpdatedAtUtc)
                .IsRequired();

            entity.HasIndex(workout => new { workout.UserId, workout.Status });

            entity.HasIndex(workout => workout.UserId)
                .HasFilter($"[{nameof(WorkoutEntity.Status)}] = {(int)WorkoutStatus.InProgress}")
                .IsUnique();
        });
    }
}
