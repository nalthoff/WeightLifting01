using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeightLifting.Api.Infrastructure.Persistence.Entities;
using WeightLifting.Api.Infrastructure.Persistence.Workouts;

namespace WeightLifting.Api.Infrastructure.Persistence.Configurations;

public sealed class WorkoutSetEntityTypeConfiguration : IEntityTypeConfiguration<WorkoutSetEntity>
{
    public void Configure(EntityTypeBuilder<WorkoutSetEntity> builder)
    {
        builder.ToTable("WorkoutSets");

        builder.HasKey(workoutSet => workoutSet.Id);

        builder.Property(workoutSet => workoutSet.SetNumber)
            .IsRequired();

        builder.Property(workoutSet => workoutSet.Reps)
            .IsRequired();

        builder.Property(workoutSet => workoutSet.Weight)
            .HasPrecision(9, 2);

        builder.Property(workoutSet => workoutSet.CreatedAtUtc)
            .IsRequired();

        builder.Property(workoutSet => workoutSet.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(workoutSet => new { workoutSet.WorkoutLiftEntryId, workoutSet.SetNumber })
            .IsUnique();

        builder.HasIndex(workoutSet => workoutSet.WorkoutId);

        builder.HasOne<WorkoutEntity>()
            .WithMany()
            .HasForeignKey(workoutSet => workoutSet.WorkoutId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WorkoutLiftEntryEntity>()
            .WithMany()
            .HasForeignKey(workoutSet => workoutSet.WorkoutLiftEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
