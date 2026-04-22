using Microsoft.AspNetCore.Mvc;
using WeightLifting.Api.Api.Contracts.Workouts;
using WeightLifting.Api.Application.Workouts.Commands.StartWorkout;
using WeightLifting.Api.Application.Workouts.Queries.GetWorkoutById;
using WeightLifting.Api.Domain.Workouts;

namespace WeightLifting.Api.Api.Controllers;

[ApiController]
[Route("api/workouts")]
public sealed class WorkoutsController(
    StartWorkoutCommandHandler startWorkoutCommandHandler,
    GetWorkoutByIdQueryHelper getWorkoutByIdQueryHelper) : ControllerBase
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    [HttpGet("{workoutId:guid}")]
    [ProducesResponseType(typeof(GetWorkoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetWorkoutResponse>> GetWorkout(
        Guid workoutId,
        CancellationToken cancellationToken = default)
    {
        var workout = await getWorkoutByIdQueryHelper.GetAsync(workoutId, DefaultUserId, cancellationToken);
        if (workout is null)
        {
            return NotFound(new
            {
                title = "Workout not found",
                status = StatusCodes.Status404NotFound,
            });
        }

        return Ok(new GetWorkoutResponse
        {
            Workout = ToWorkoutSummary(workout),
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(StartWorkoutCreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ExistingInProgressWorkoutResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<StartWorkoutCreatedResponse>> StartWorkout(
        [FromBody] StartWorkoutRequest? request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await startWorkoutCommandHandler.HandleAsync(
                new StartWorkoutCommand
                {
                    Label = request?.Label,
                },
                cancellationToken);

            if (result.Outcome == StartWorkoutOutcome.AlreadyInProgress)
            {
                return Conflict(new ExistingInProgressWorkoutResponse
                {
                    Title = "Workout already in progress",
                    Status = StatusCodes.Status409Conflict,
                    Workout = ToWorkoutSummary(result.Workout),
                });
            }

            return Created(
                $"/api/workouts/{result.Workout.Id}",
                new StartWorkoutCreatedResponse
                {
                    Workout = ToWorkoutSummary(result.Workout),
                });
        }
        catch (ArgumentException)
        {
            return UnprocessableEntity(CreateLabelValidationResponse());
        }
    }

    private static WorkoutSessionSummaryResponse ToWorkoutSummary(Workout workout) => new()
    {
        Id = workout.Id,
        Status = workout.Status.ToString(),
        Label = workout.Label,
        StartedAtUtc = workout.StartedAtUtc,
    };

    private static object CreateLabelValidationResponse() => new
    {
        title = "Validation failed",
        status = StatusCodes.Status422UnprocessableEntity,
        errors = new Dictionary<string, string[]>
        {
            ["label"] = [$"Workout label must be {Workout.MaxLabelLength} characters or fewer."],
        },
    };
}
