using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UMS.Application.Features.Coordinator.Commands.ReviewApplication;
using UMS.Application.Features.Coordinator.Queries.GetAvailableUshersQuery;
using UMS.Application.Features.Coordinator.Queries.GetConfirmed;
using UMS.Application.Features.Coordinator.Queries.GetScheduleRoster;
using UMS.Application.Features.Events.Commands.InviteUsher;
using UMS.Application.Features.Events.Queries.GetCoordinatorSchedules;
using UMS.Application.Features.Events.Queries.GetScheduleInvitations;
using UMS.Contracts.Coordinator;
using UMS.Contracts.Events;
using UMS.Contracts.Usher;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.api.Controllers
{

    [ApiController]
    [Route("api/coordinator")]
    [Produces("application/json")]
    [Authorize(Roles = "EVENT_COORDINATOR")]
    public sealed class CoordinatorController(ISender sender) : ControllerBase
    {
        private Guid CoordinatorId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("schedules")]
        [ProducesResponseType(typeof(GetCoordinatorSchedulesResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMySchedules(CancellationToken ct)
        {
            var result = await sender.Send(
                new GetCoordinatorSchedulesQuery(CoordinatorId), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : StatusCode(StatusCodes.Status502BadGateway, result.Error);
        }

        [HttpPost("schedules/invite")]
        [ProducesResponseType(typeof(InviteUsherResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> InviteUsher(

            [FromBody] InviteUsherRequest request,
            CancellationToken ct)
        {
            var result = await sender.Send(new InviteUsherCommand(
      ExternalScheduleId: request.scheduleId,
      ExternalEventId: request.eventId,
      UsherId: request.UsherId,
      CoordinatorId: CoordinatorId), ct);
            if (result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status201Created, result.Value);
            }
            return result.Error.Code switch
            {
                "INVITE_001" => NotFound(result.Error),
                "INVITE_002" => NotFound(result.Error),
                "INVITE_003" => StatusCode(StatusCodes.Status403Forbidden, result.Error),
                "INVITE_004" => Conflict(result.Error),
                "INVITE_005" => Conflict(result.Error),
                "INVITE_008" => BadRequest(result.Error),
                "SCHEDULE_005" => StatusCode(StatusCodes.Status502BadGateway, result.Error),
                _ => BadRequest(result.Error)
            };
        }

        [HttpGet("schedules/{eventId}/{scheduleId}/invitations")]
        [ProducesResponseType(typeof(GetScheduleInvitationsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetScheduleInvitaions(
              string eventId,
              string scheduleId,
              [FromQuery] int page = 1,
              [FromQuery] int size = 10,
              [FromQuery] InvitationStatus? status = null,
              CancellationToken ct = default
            )
        {
            var result = await sender.Send(new GetScheduleInvitationsQuery(
        ExternalScheduleId: scheduleId,
        ExternalEventId: eventId,
        CoordinatorId: CoordinatorId,
        Page: page,
        Size: size,
        Status: status), ct);

            return result.IsSuccess
    ? Ok(result.Value)
    : result.Error.Code switch
    {
        "INVITE_003" => StatusCode(StatusCodes.Status403Forbidden, result.Error),
        _ => BadRequest(result.Error)
    };

        }

        [HttpGet("schedules/{eventId}/{scheduleId}/roster")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoster(
               string eventId,
               string scheduleId,
               [FromQuery] CoordinatorScheduleFilter filter,
               [FromQuery] int page = 1,
               [FromQuery] int size = 10,
               CancellationToken ct = default)
        {
            var result = await sender.Send(new GetScheduleRosterQuery(
                CoordinatorId: CoordinatorId,
                ExternalEventId: eventId,
                ExternalScheduleId: scheduleId,
                Filter: filter,
                Page: page,
                Size: size), ct);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpPost("applications/review")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReviewApplication(
       ApplicationReviewRequest request,
       CancellationToken ct = default)
        {
            var result = await sender.Send(new ReviewApplicationCommand(
                CoordinatorId: CoordinatorId,
                ApplicationId: request.ApplicationId,
                Approve: request.Accept), ct);

            return result.IsSuccess
                ? Ok(new { message = request.Accept ? "Application approved." : "Application rejected." })
                : result.Error.Code switch
                {
                    "USHER_SCH_006" => NotFound(result.Error),
                    "SCHEDULE_008" => Forbid(),
                    _ => BadRequest(result.Error)
                };
        }
        [HttpGet("schedules/{eventId}/{scheduleId}/available-ushers")]
        [ProducesResponseType(typeof(PagedAvailableUshersResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableUshers(
       string eventId,
       string scheduleId,
       [FromQuery] int page = 1,
       [FromQuery] int size = 10,
       CancellationToken ct = default)
        {
            var result = await sender.Send(new AvailableUshersQuery(
                ExternalScheduleId: scheduleId,
                ExternalEventId: eventId,
                CoordinatorId: CoordinatorId,
                Page: page,
                Size: size), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.Code switch
                {
                    "SCHEDULE_002" => NotFound(result.Error),
                    "SCHEDULE_005" => StatusCode(StatusCodes.Status502BadGateway, result.Error),
                    _ => BadRequest(result.Error)
                };
        }
        [HttpGet("schedules/{eventId}/{scheduleId}/confirmed")]
        [ProducesResponseType(typeof(PagedConfirmedRosterResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConfirmedRoster(
       string eventId,
       string scheduleId,
       [FromQuery] int page = 1,
       [FromQuery] int size = 10,
       CancellationToken ct = default)
        {
            var result = await sender.Send(new ConfirmedRosterQuery(
                ExternalScheduleId: scheduleId,
                CoordinatorId: CoordinatorId,
                Page: page,
                Size: size), ct);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
    }
}