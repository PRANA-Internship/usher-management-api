using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UMS.Application.Features.Events.Commands.AssignCoordinator;
using UMS.Application.Features.Events.Commands.RemoveCoordinator;
using UMS.Application.Features.Events.Queries.GetEvents;
using UMS.Contracts.Events;
using UMS.Domain.Common;


namespace UMS.api.Controllers
{

    [ApiController]
    [Route("api/events")]
    [Produces("application/json")]
    [Authorize(Roles = "ADMIN")]
    public sealed class EventsController(ISender sender) : ControllerBase
    {

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedEventsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetEvents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var validatedPageNumber = pageNumber < 1 ? 1 : pageNumber;
            var validatedPageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

            var result = await sender.Send(new GetEventsQuery(validatedPageNumber, validatedPageSize), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : StatusCode(StatusCodes.Status502BadGateway, result.Error);
        }

        [HttpGet("{eventId}/schedules")]
        [ProducesResponseType(typeof(EventSchedulesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetEventSchedules(
            string eventId, CancellationToken ct)
        {
            var result = await sender.Send(new GetEventSchedulesQuery(eventId), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.Code switch
                {
                    "SCHEDULE_001" => NotFound(result.Error),
                    "SCHEDULE_005" => StatusCode(StatusCodes.Status502BadGateway, result.Error),
                    _ => BadRequest(result.Error)
                };
        }

        [HttpPost("schedules/assign")]
        [ProducesResponseType(typeof(AssignCoordinatorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignCoordinator(
            [FromBody] AssignCoordinatorRequest request,
            CancellationToken ct)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await sender.Send(new AssignCoordinatorCommand(
                ExternalEventId: request.eventId,
                ExternalScheduleId: request.scheduleId,
                CoordinatorId: request.CoordinatorId,
                AdminId: adminId), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.Code switch
                {
                    "SCHEDULE_002" => NotFound(result.Error),
                    "SCHEDULE_003" => NotFound(result.Error),
                    "SCHEDULE_005" => StatusCode(StatusCodes.Status502BadGateway, result.Error),
                    "SCHEDULE_006" => BadRequest(result.Error),
                    _ => BadRequest(result.Error)
                };
        }

        [HttpDelete("schedules/remove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UnassignCoordinator(
            [FromBody] RemoveCoordinatorRequest request,
            CancellationToken ct)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await sender.Send(new RemoveCoordinatorCommand(
                ExternalEventId: request.eventId,
                ExternalScheduleId: request.scheduleId,
                AdminId: adminId), ct);

            return result.IsSuccess
                ? Ok(new { Message = "Coordinator unassigned successfully", ScheduleId = request.scheduleId })
                : result.Error.Code switch
                {
                    "SCHEDULE_002" => NotFound(result.Error),
                    "SCHEDULE_008" => BadRequest(result.Error),
                    _ => BadRequest(result.Error)
                };
        }
    }
}