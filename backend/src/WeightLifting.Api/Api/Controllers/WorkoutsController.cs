using Microsoft.AspNetCore.Mvc;
using WeightLifting.Api.Api.Contracts.Workouts;
using WeightLifting.Api.Application.Workouts;
using WeightLifting.Api.Application.Workouts.Commands.AddWorkoutLift;
using WeightLifting.Api.Application.Workouts.Commands.CompleteWorkout;
using WeightLifting.Api.Application.Workouts.Queries.GetActiveWorkoutSummary;
using WeightLifting.Api.Application.Workouts.Commands.StartWorkout;
using WeightLifting.Api.Application.Workouts.Queries.GetWorkoutById;
using WeightLifting.Api.Application.Workouts.Queries.ListWorkoutLifts;
using WeightLifting.Api.Domain.Workouts;

namespace WeightLifting.Api.Api.Controllers;

[ApiController]
[Route("api/workouts")]
public sealed class WorkoutsController(
    AddWorkoutLiftCommandHandler addWorkoutLiftCommandHandler,
    CompleteWorkoutCommandHandler completeWorkoutCommandHandler,
    GetActiveWorkoutSummaryQueryHelper getActiveWorkoutSummaryQueryHelper,
    ListWorkoutLiftsQueryHelper listWorkoutLiftsQueryHelper,
    StartWorkoutCommandHandler startWorkoutCommandHandler,
    GetWorkoutByIdQueryHelper getWorkoutByIdQueryHelper) : ControllerBase
{
    // Placeholder identity until auth context is wired.
    private const string DefaultUserId = "default-user";

    [HttpGet("active")]
    [ProducesResponseType(typeof(ActiveWorkoutSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<ActiveWorkoutSummaryResponse>> GetActiveWorkoutSummary(
        CancellationToken cancellationToken = default)
    {
        var workout = await getActiveWorkoutSummaryQueryHelper.GetAsync(cancellationToken);
        if (workout is null)
        {
            return NoContent();
        }

        return Ok(new ActiveWorkoutSummaryResponse
        {
            Workout = ToWorkoutSummary(workout),
        });
    }

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

    [HttpPost("{workoutId:guid}/complete")]
    [ProducesResponseType(typeof(CompleteWorkoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompleteWorkoutResponse>> CompleteWorkout(
        Guid workoutId,
        CancellationToken cancellationToken = default)
    {
        var result = await completeWorkoutCommandHandler.HandleAsync(
            new CompleteWorkoutCommand
            {
                WorkoutId = workoutId,
            },
            cancellationToken);

        if (result.Outcome == CompleteWorkoutOutcome.NotFound)
        {
            return NotFound(new
            {
                title = "Workout not found",
                status = StatusCodes.Status404NotFound,
            });
        }

        if (result.Outcome == CompleteWorkoutOutcome.Conflict)
        {
            return Conflict(new
            {
                title = "Workout cannot be completed",
                status = StatusCodes.Status409Conflict,
                errors = new Dictionary<string, string[]>
                {
                    ["workout"] = ["Workout must be in progress to complete."],
                },
            });
        }

        return Ok(new CompleteWorkoutResponse
        {
            Workout = ToWorkoutSummary(result.Workout!),
        });
    }

    [HttpGet("{workoutId:guid}/lifts")]
    [ProducesResponseType(typeof(WorkoutLiftListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutLiftListResponse>> ListWorkoutLifts(
        Guid workoutId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var workoutLifts = await listWorkoutLiftsQueryHelper.GetAsync(workoutId, cancellationToken);
            return Ok(new WorkoutLiftListResponse
            {
                Items = workoutLifts.Select(ToWorkoutLiftEntryResponse).ToList(),
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                title = "Workout not found",
                status = StatusCodes.Status404NotFound,
            });
        }
    }

    [HttpPost("{workoutId:guid}/lifts")]
    [ProducesResponseType(typeof(AddWorkoutLiftResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AddWorkoutLiftResponse>> AddWorkoutLift(
        Guid workoutId,
        [FromBody] AddWorkoutLiftRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || request.LiftId == Guid.Empty)
        {
            return UnprocessableEntity(CreateLiftIdValidationResponse());
        }

        try
        {
            var result = await addWorkoutLiftCommandHandler.HandleAsync(
                new AddWorkoutLiftCommand
                {
                    WorkoutId = workoutId,
                    LiftId = request.LiftId,
                },
                cancellationToken);

            return Created(
                $"/api/workouts/{workoutId}/lifts/{result.WorkoutLift.Id}",
                new AddWorkoutLiftResponse
                {
                    WorkoutLift = ToWorkoutLiftEntryResponse(result.WorkoutLift),
                });
        }
        catch (WorkoutNotInProgressException)
        {
            return Conflict(new
            {
                title = "Workout cannot accept lifts",
                status = StatusCodes.Status409Conflict,
                errors = new Dictionary<string, string[]>
                {
                    ["workout"] = ["Workout must be in progress to add lifts."],
                },
            });
        }
        catch (LiftNotActiveException)
        {
            return UnprocessableEntity(new
            {
                title = "Validation failed",
                status = StatusCodes.Status422UnprocessableEntity,
                errors = new Dictionary<string, string[]>
                {
                    ["liftId"] = ["Lift must be active to add to the workout."],
                },
            });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                title = "Resource not found",
                status = StatusCodes.Status404NotFound,
                detail = exception.Message,
            });
        }
    }

    private static WorkoutSessionSummaryResponse ToWorkoutSummary(Workout workout) => new()
    {
        Id = workout.Id,
        Status = workout.Status.ToString(),
        Label = workout.Label,
        StartedAtUtc = workout.StartedAtUtc,
        CompletedAtUtc = workout.CompletedAtUtc,
    };

    private static WorkoutLiftEntryResponse ToWorkoutLiftEntryResponse(WorkoutLiftEntry workoutLiftEntry) => new()
    {
        Id = workoutLiftEntry.Id,
        WorkoutId = workoutLiftEntry.WorkoutId,
        LiftId = workoutLiftEntry.LiftId,
        DisplayName = workoutLiftEntry.DisplayName,
        AddedAtUtc = workoutLiftEntry.AddedAtUtc,
        Position = workoutLiftEntry.Position,
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

    private static object CreateLiftIdValidationResponse() => new
    {
        title = "Validation failed",
        status = StatusCodes.Status422UnprocessableEntity,
        errors = new Dictionary<string, string[]>
        {
            ["liftId"] = ["Lift id is required."],
        },
    };
}
