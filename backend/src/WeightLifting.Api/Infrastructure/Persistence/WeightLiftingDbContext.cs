using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Infrastructure.Persistence.Lifts;

namespace WeightLifting.Api.Infrastructure.Persistence;

public sealed class WeightLiftingDbContext(DbContextOptions<WeightLiftingDbContext> options)
    : DbContext(options)
{
    public DbSet<LiftEntity> Lifts => Set<LiftEntity>();

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
    }
}
