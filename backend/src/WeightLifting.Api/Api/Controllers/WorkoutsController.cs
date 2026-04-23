using Microsoft.AspNetCore.Mvc;
using WeightLifting.Api.Api.Contracts.Workouts;
using WeightLifting.Api.Application.Workouts;
using WeightLifting.Api.Application.Workouts.Commands.AddWorkoutLift;
using WeightLifting.Api.Application.Workouts.Commands.AddWorkoutSet;
using WeightLifting.Api.Application.Workouts.Commands.CompleteWorkout;
using WeightLifting.Api.Application.Workouts.Commands.DeleteWorkoutSet;
using WeightLifting.Api.Application.Workouts.Commands.ReorderWorkoutLifts;
using WeightLifting.Api.Application.Workouts.Commands.RemoveWorkoutLift;
using WeightLifting.Api.Application.Workouts.Commands.UpdateWorkoutSet;
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
    AddWorkoutSetCommandHandler addWorkoutSetCommandHandler,
    UpdateWorkoutSetCommandHandler updateWorkoutSetCommandHandler,
    DeleteWorkoutSetCommandHandler deleteWorkoutSetCommandHandler,
    CompleteWorkoutCommandHandler completeWorkoutCommandHandler,
    ReorderWorkoutLiftsCommandHandler reorderWorkoutLiftsCommandHandler,
    RemoveWorkoutLiftCommandHandler removeWorkoutLiftCommandHandler,
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

    [HttpPost("{workoutId:guid}/lifts/{workoutLiftEntryId:guid}/sets")]
    [ProducesResponseType(typeof(CreateWorkoutSetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CreateWorkoutSetResponse>> AddWorkoutSet(
        Guid workoutId,
        Guid workoutLiftEntryId,
        [FromBody] CreateWorkoutSetRequest? request,
        CancellationToken cancellationToken = default)
    {
        var result = await addWorkoutSetCommandHandler.HandleAsync(
            new AddWorkoutSetCommand
            {
                WorkoutId = workoutId,
                WorkoutLiftEntryId = workoutLiftEntryId,
                Reps = request?.Reps ?? 0,
                Weight = request?.Weight,
            },
            cancellationToken);

        if (result.Outcome == AddWorkoutSetOutcome.NotFound)
        {
            return NotFound(new
            {
                title = "Resource not found",
                status = StatusCodes.Status404NotFound,
            });
        }

        if (result.Outcome == AddWorkoutSetOutcome.Conflict)
        {
            return Conflict(new
            {
                title = "Workout cannot accept sets",
                status = StatusCodes.Status409Conflict,
                errors = result.Errors,
            });
        }

        if (result.Outcome == AddWorkoutSetOutcome.ValidationFailed)
        {
            return UnprocessableEntity(new
            {
                title = "Validation failed",
                status = StatusCodes.Status422UnprocessableEntity,
                errors = result.Errors,
            });
        }

        return Created(
            $"/api/workouts/{workoutId}/lifts/{workoutLiftEntryId}/sets/{result.Set!.Id}",
            new CreateWorkoutSetResponse
            {
                WorkoutId = workoutId,
                WorkoutLiftEntryId = workoutLiftEntryId,
                Set = ToWorkoutSetEntryResponse(result.Set),
            });
    }

    [HttpPut("{workoutId:guid}/lifts/{workoutLiftEntryId:guid}/sets/{setId:guid}")]
    [ProducesResponseType(typeof(UpdateWorkoutSetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UpdateWorkoutSetResponse>> UpdateWorkoutSet(
        Guid workoutId,
        Guid workoutLiftEntryId,
        Guid setId,
        [FromBody] UpdateWorkoutSetRequest? request,
        CancellationToken cancellationToken = default)
    {
        var result = await updateWorkoutSetCommandHandler.HandleAsync(
            new UpdateWorkoutSetCommand
            {
                WorkoutId = workoutId,
                WorkoutLiftEntryId = workoutLiftEntryId,
                SetId = setId,
                Reps = request?.Reps ?? 0,
                Weight = request?.Weight,
            },
            cancellationToken);

        if (result.Outcome == UpdateWorkoutSetOutcome.NotFound)
        {
            return NotFound(new
            {
                title = "Resource not found",
                status = StatusCodes.Status404NotFound,
            });
        }

        if (result.Outcome == UpdateWorkoutSetOutcome.Conflict)
        {
            return Conflict(new
            {
                title = "Workout cannot update sets",
                status = StatusCodes.Status409Conflict,
                errors = result.Errors,
            });
        }

        if (result.Outcome == UpdateWorkoutSetOutcome.ValidationFailed)
        {
            return UnprocessableEntity(new
            {
                title = "Validation failed",
                status = StatusCodes.Status422UnprocessableEntity,
                errors = result.Errors,
            });
        }

        return Ok(new UpdateWorkoutSetResponse
        {
            WorkoutId = workoutId,
            WorkoutLiftEntryId = workoutLiftEntryId,
            Set = ToWorkoutSetEntryResponse(result.Set!),
        });
    }

    [HttpDelete("{workoutId:guid}/lifts/{workoutLiftEntryId:guid}/sets/{setId:guid}")]
    [ProducesResponseType(typeof(DeleteWorkoutSetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<DeleteWorkoutSetResponse>> DeleteWorkoutSet(
        Guid workoutId,
        Guid workoutLiftEntryId,
        Guid setId,
        CancellationToken cancellationToken = default)
    {
        if (workoutId == Guid.Empty || workoutLiftEntryId == Guid.Empty || setId == Guid.Empty)
        {
            return UnprocessableEntity(CreateDeleteWorkoutSetValidationResponse(workoutId, workoutLiftEntryId, setId));
        }

        var result = await deleteWorkoutSetCommandHandler.HandleAsync(
            new DeleteWorkoutSetCommand
            {
                WorkoutId = workoutId,
                WorkoutLiftEntryId = workoutLiftEntryId,
                SetId = setId,
            },
            cancellationToken);

        if (result.Outcome == DeleteWorkoutSetOutcome.NotFound)
        {
            return NotFound(new
            {
                title = "Resource not found",
                status = StatusCodes.Status404NotFound,
            });
        }

        if (result.Outcome == DeleteWorkoutSetOutcome.Conflict)
        {
            return Conflict(new
            {
                title = "Workout cannot remove sets",
                status = StatusCodes.Status409Conflict,
                errors = new Dictionary<string, string[]>
                {
                    ["workout"] = ["Workout must be in progress to remove sets."],
                },
            });
        }

        return Ok(new DeleteWorkoutSetResponse
        {
            WorkoutId = result.WorkoutId,
            WorkoutLiftEntryId = result.WorkoutLiftEntryId,
            SetId = result.SetId,
        });
    }

    [HttpDelete("{workoutId:guid}/lifts/{workoutLiftEntryId:guid}")]
    [ProducesResponseType(typeof(RemoveWorkoutLiftResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RemoveWorkoutLiftResponse>> RemoveWorkoutLift(
        Guid workoutId,
        Guid workoutLiftEntryId,
        CancellationToken cancellationToken = default)
    {
        if (workoutId == Guid.Empty || workoutLiftEntryId == Guid.Empty)
        {
            return UnprocessableEntity(CreateRemoveWorkoutLiftValidationResponse(workoutId, workoutLiftEntryId));
        }

        var result = await removeWorkoutLiftCommandHandler.HandleAsync(
            new RemoveWorkoutLiftCommand
            {
                WorkoutId = workoutId,
                WorkoutLiftEntryId = workoutLiftEntryId,
            },
            cancellationToken);

        if (result.Outcome == RemoveWorkoutLiftOutcome.NotFound)
        {
            return NotFound(new
            {
                title = "Resource not found",
                status = StatusCodes.Status404NotFound,
            });
        }

        if (result.Outcome == RemoveWorkoutLiftOutcome.Conflict)
        {
            return Conflict(new
            {
                title = "Workout cannot remove lifts",
                status = StatusCodes.Status409Conflict,
                errors = new Dictionary<string, string[]>
                {
                    ["workout"] = ["Workout must be in progress to remove lifts."],
                },
            });
        }

        return Ok(new RemoveWorkoutLiftResponse
        {
            WorkoutId = result.WorkoutId,
            WorkoutLiftEntryId = result.WorkoutLiftEntryId,
        });
    }

    [HttpPut("{workoutId:guid}/lifts/reorder")]
    [ProducesResponseType(typeof(ReorderWorkoutLiftsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ReorderWorkoutLiftsResponse>> ReorderWorkoutLifts(
        Guid workoutId,
        [FromBody] ReorderWorkoutLiftsRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (workoutId == Guid.Empty || request is null || request.OrderedWorkoutLiftEntryIds.Count == 0)
        {
            return UnprocessableEntity(CreateReorderWorkoutLiftsValidationResponse(workoutId));
        }

        var result = await reorderWorkoutLiftsCommandHandler.HandleAsync(
            new ReorderWorkoutLiftsCommand
            {
                WorkoutId = workoutId,
                OrderedWorkoutLiftEntryIds = request.OrderedWorkoutLiftEntryIds,
            },
            cancellationToken);

        if (result.Outcome == ReorderWorkoutLiftsOutcome.NotFound)
        {
            return NotFound(new
            {
                title = "Resource not found",
                status = StatusCodes.Status404NotFound,
            });
        }

        if (result.Outcome == ReorderWorkoutLiftsOutcome.Conflict)
        {
            return Conflict(new
            {
                title = "Workout cannot reorder lifts",
                status = StatusCodes.Status409Conflict,
                errors = new Dictionary<string, string[]>
                {
                    ["workout"] = ["Workout must be in progress to reorder lifts."],
                },
            });
        }

        if (result.Outcome == ReorderWorkoutLiftsOutcome.ValidationFailed)
        {
            return UnprocessableEntity(CreateReorderWorkoutLiftsValidationResponse(workoutId));
        }

        return Ok(new ReorderWorkoutLiftsResponse
        {
            WorkoutId = result.WorkoutId,
            Items = result.Items.Select(ToWorkoutLiftEntryResponse).ToList(),
        });
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
        Sets = workoutLiftEntry.Sets.Select(ToWorkoutSetEntryResponse).ToList(),
    };

    private static WorkoutSetEntryResponse ToWorkoutSetEntryResponse(WorkoutSetEntry workoutSetEntry) => new()
    {
        Id = workoutSetEntry.Id,
        WorkoutLiftEntryId = workoutSetEntry.WorkoutLiftEntryId,
        SetNumber = workoutSetEntry.SetNumber,
        Reps = workoutSetEntry.Reps,
        Weight = workoutSetEntry.Weight,
        CreatedAtUtc = workoutSetEntry.CreatedAtUtc,
        UpdatedAtUtc = workoutSetEntry.UpdatedAtUtc,
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

    private static object CreateRemoveWorkoutLiftValidationResponse(
        Guid workoutId,
        Guid workoutLiftEntryId)
    {
        var errors = new Dictionary<string, string[]>();
        if (workoutId == Guid.Empty)
        {
            errors["workoutId"] = ["Workout id is required."];
        }

        if (workoutLiftEntryId == Guid.Empty)
        {
            errors["workoutLiftEntryId"] = ["Workout lift entry id is required."];
        }

        return new
        {
            title = "Validation failed",
            status = StatusCodes.Status422UnprocessableEntity,
            errors,
        };
    }

    private static object CreateDeleteWorkoutSetValidationResponse(
        Guid workoutId,
        Guid workoutLiftEntryId,
        Guid setId)
    {
        var errors = new Dictionary<string, string[]>();
        if (workoutId == Guid.Empty)
        {
            errors["workoutId"] = ["Workout id is required."];
        }

        if (workoutLiftEntryId == Guid.Empty)
        {
            errors["workoutLiftEntryId"] = ["Workout lift entry id is required."];
        }

        if (setId == Guid.Empty)
        {
            errors["setId"] = ["Set id is required."];
        }

        return new
        {
            title = "Validation failed",
            status = StatusCodes.Status422UnprocessableEntity,
            errors,
        };
    }

    private static object CreateReorderWorkoutLiftsValidationResponse(Guid workoutId)
    {
        var errors = new Dictionary<string, string[]>();
        if (workoutId == Guid.Empty)
        {
            errors["workoutId"] = ["Workout id is required."];
        }

        errors["orderedWorkoutLiftEntryIds"] = ["A complete ordered set of workout lift entry ids is required."];

        return new
        {
            title = "Validation failed",
            status = StatusCodes.Status422UnprocessableEntity,
            errors,
        };
    }
}
