using Microsoft.EntityFrameworkCore;
using WeightLifting.Api.Domain.Workouts;
using WeightLifting.Api.Infrastructure.Persistence;

namespace WeightLifting.Api.Application.Workouts.Queries.ListCompletedWorkouts;

public sealed class ListCompletedWorkoutsQueryHelper(WeightLiftingDbContext dbContext)
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";
    private const string DurationFallback = "00:00";

    public async Task<IReadOnlyList<CompletedWorkoutHistoryItem>> GetAsync(CancellationToken cancellationToken)
    {
        var rows = await dbContext.Workouts
            .AsNoTracking()
            .Where(workout =>
                workout.UserId == DefaultUserId
                && workout.Status == WorkoutStatus.Completed
                && workout.CompletedAtUtc.HasValue)
            .OrderByDescending(workout => workout.CompletedAtUtc)
            .Select(workout => new
            {
                WorkoutId = workout.Id,
                Label = workout.Label,
                StartedAtUtc = workout.StartedAtUtc,
                CompletedAtUtc = workout.CompletedAtUtc!.Value,
                LiftCount = dbContext.WorkoutLiftEntries.Count(lift => lift.WorkoutId == workout.Id),
            })
            .ToListAsync(cancellationToken);

        return rows.Select(row => new CompletedWorkoutHistoryItem
        {
            WorkoutId = row.WorkoutId,
            Label = row.Label,
            CompletedAtUtc = row.CompletedAtUtc,
            DurationDisplay = FormatDurationDisplay(row.StartedAtUtc, row.CompletedAtUtc),
            LiftCount = row.LiftCount,
        }).ToList();
    }

    private static string FormatDurationDisplay(DateTime startedAtUtc, DateTime completedAtUtc)
    {
        if (startedAtUtc == default || completedAtUtc == default || completedAtUtc < startedAtUtc)
        {
            return DurationFallback;
        }

        var duration = completedAtUtc - startedAtUtc;
        var totalHours = (int)duration.TotalHours;
        return $"{totalHours:D2}:{duration.Minutes:D2}";
    }
}
