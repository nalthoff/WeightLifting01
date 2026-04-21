using Microsoft.AspNetCore.Mvc;
using WeightLifting.Api.Api.Contracts.Lifts;
using WeightLifting.Api.Application.Lifts.Commands.CreateLift;
using WeightLifting.Api.Application.Lifts.Commands.RenameLift;
using WeightLifting.Api.Application.Lifts.Queries.GetLifts;
using WeightLifting.Api.Domain.Lifts;

namespace WeightLifting.Api.Api.Controllers;

[ApiController]
[Route("api/lifts")]
public sealed class LiftsController(
    CreateLiftCommandHandler createLiftCommandHandler,
    RenameLiftCommandHandler renameLiftCommandHandler,
    GetLiftsQueryHandler getLiftsQueryHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(LiftListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LiftListResponse>> GetLifts(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var lifts = await getLiftsQueryHandler.HandleAsync(new GetLiftsQuery
        {
            ActiveOnly = activeOnly,
        }, cancellationToken);

        return Ok(new LiftListResponse
        {
            Items = lifts.Select(ToListItemResponse).ToList(),
            LastSyncedAtUtc = lifts.Count == 0 ? null : lifts.Max(lift => lift.CreatedAtUtc),
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateLiftResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CreateLiftResponse>> CreateLift(
        [FromBody] CreateLiftRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var lift = await createLiftCommandHandler.HandleAsync(new CreateLiftCommand
            {
                Name = request.Name,
                ClientRequestId = request.ClientRequestId,
            }, cancellationToken);

            var response = new CreateLiftResponse
            {
                Lift = ToLiftResponse(lift),
            };

            return Created($"/api/lifts/{lift.Id}", response);
        }
        catch (ArgumentException)
        {
            return UnprocessableEntity(CreateNameValidationResponse());
        }
    }

    [HttpPut("{liftId:guid}")]
    [ProducesResponseType(typeof(RenameLiftResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RenameLiftResponse>> RenameLift(
        Guid liftId,
        [FromBody] RenameLiftRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var lift = await renameLiftCommandHandler.HandleAsync(new RenameLiftCommand
            {
                LiftId = liftId,
                Name = request.Name,
            }, cancellationToken);

            return Ok(new RenameLiftResponse
            {
                Lift = ToLiftResponse(lift),
            });
        }
        catch (ArgumentException)
        {
            return UnprocessableEntity(CreateNameValidationResponse());
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                title = "Lift not found",
                status = StatusCodes.Status404NotFound,
            });
        }
    }

    private static LiftResponse ToLiftResponse(Lift lift) => new()
    {
        Id = lift.Id,
        Name = lift.Name,
        IsActive = lift.IsActive,
        CreatedAtUtc = lift.CreatedAtUtc,
    };

    private static LiftListItemResponse ToListItemResponse(Lift lift) => new()
    {
        Id = lift.Id,
        Name = lift.Name,
        IsActive = lift.IsActive,
    };

    private static object CreateNameValidationResponse() => new
    {
        title = "Validation failed",
        status = StatusCodes.Status422UnprocessableEntity,
        errors = new Dictionary<string, string[]>
        {
            ["name"] = ["Lift name is required."],
        },
    };
}
