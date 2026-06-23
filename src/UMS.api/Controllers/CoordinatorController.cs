using System;
using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UMS.Application.Features.Coordinator.Commands.MarkAttendance;
using UMS.Application.Features.Coordinator.Commands.PerformanceReview;
using UMS.Application.Features.Coordinator.Commands.ReviewApplication;
using UMS.Application.Features.Coordinator.Commands.UpdateProfile;
using UMS.Application.Features.Coordinator.Queries.AttendanceSheet;
using UMS.Application.Features.Coordinator.Queries.DashboardAnalytics;
using UMS.Application.Features.Coordinator.Queries.GetAvailableUshersQuery;
using UMS.Application.Features.Coordinator.Queries.GetConfirmed;
using UMS.Application.Features.Coordinator.Queries.GetMyProfile;
using UMS.Application.Features.Coordinator.Queries.GetScheduleRoster;
using UMS.Application.Features.Coordinator.Queries.PerformanceReviewList;
using UMS.Application.Features.Coordinator.Queries.UsherDetail;
using UMS.Application.Features.Coordinator.Queries.UsherEventHistory;
using UMS.Application.Features.Events.Commands.InviteUsher;
using UMS.Application.Features.Events.Queries.GetCoordinatorSchedules;
using UMS.Application.Features.Events.Queries.GetScheduleInvitations;
using UMS.Contracts.Coordinator;
using UMS.Contracts.Coordinator.Attendance;
using UMS.Contracts.Coordinator.Dashboard;
using UMS.Contracts.Coordinator.Performance;
using UMS.Contracts.Coordinator.Usher;
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

        [HttpGet("me")]
        [ProducesResponseType(typeof(GetMyProfileCoordinator), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile(
            CancellationToken ct)
        {
            var result = await sender.Send(
                new GetMyProfileQuery(CoordinatorId), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : NotFound(result.Error);
        }

        [HttpPatch("me/update-profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(
          [FromBody] UpdateCoordinatorProfileRequest request,
           CancellationToken ct)
        {
            var result = await sender.Send(new UpdateCoordinatorProfileCommand(
                UserId: CoordinatorId,
                FullName: request.FullName,
                Phone: request.Phone,
                CurrentPassword: request.CurrentPassword,
                NewPassword: request.NewPassword), ct);

            return result.IsSuccess
                ? Ok(new { message = "Profile updated successfully." })
                : result.Error.Code switch
                {
                    "USER_NOT_FOUND" => NotFound(result.Error),
                    "AUTH_010" => BadRequest(result.Error),
                    _ => BadRequest(result.Error)
                };
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
        [HttpGet("schedules/{eventId}/{scheduleId}/attendance")]
        [ProducesResponseType(typeof(AttendanceSheetResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAttendanceSheet(
    string eventId,
    string scheduleId,
    [FromQuery] DateOnly date,
    [FromQuery] DayStatus dayStatus,
    CancellationToken ct = default)
        {
            var result = await sender.Send(new AttendanceSheetQuery(
                CoordinatorId: CoordinatorId,
                ExternalEventId: eventId,
                ExternalScheduleId: scheduleId,
                AttendanceDate: date,
                DayStatus: dayStatus), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.Code switch
                {
                    "ATT_003" => BadRequest(result.Error),
                    "ATT_006" => Forbid(),
                    "SCHEDULE_002" => NotFound(result.Error),
                    _ => BadRequest(result.Error)
                };
        }
        [HttpPost("schedules/attendance")]
        [ProducesResponseType(typeof(MarkAttendanceResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAttendance(
    [FromBody] MarkAttendanceRequest request,
    CancellationToken ct = default)
        {
            if (!Enum.IsDefined(typeof(AttendanceStatus), request.Status) ||
                request.Status == (int)AttendanceStatus.NotMarked)
                return BadRequest("Status must be 0 (Absent), 1 (Late), or 2 (Present).");

            var result = await sender.Send(new MarkAttendanceCommand(
                CoordinatorId: CoordinatorId,
                ExternalEventId: request.eventId,
                ExternalScheduleId: request.scheduleId,
                AttendanceDate: DateOnly.FromDateTime(DateTime.UtcNow),
                DayStatus: request.DayStatus,
                UsherId: request.UsherId,
                Status: (AttendanceStatus)request.Status), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.Code switch
                {
                    "ATT_004" => BadRequest(result.Error),
                    "ATT_006" => Forbid(),
                    "ATT_007" => BadRequest(result.Error),
                    "SCHEDULE_002" => NotFound(result.Error),
                    _ => BadRequest(result.Error)
                };
        }

        [HttpGet("dashboard/analytics")]
        [ProducesResponseType(typeof(CoordinatorDashboardResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboardAnalytics(CancellationToken ct)
        {
            var result = await sender.Send(new GetCoordinatorDashboardAnalyticsQuery(), ct);
            return result.IsSuccess ? Ok(result.Value) : StatusCode(StatusCodes.Status502BadGateway, result.Error);
        }

        [HttpGet("schedules/{eventId}/{scheduleId}/reviews")]
        [ProducesResponseType(typeof(PerformanceReviewListResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPerformanceReviews(
            string eventId,
            string scheduleId,
            CancellationToken ct = default)
        {
            var result = await sender.Send(new PerformanceReviewListQuery(
                CoordinatorId: CoordinatorId,
                ExternalEventId: eventId,
                ExternalScheduleId: scheduleId), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.Code switch
                {
                    "REVIEW_005" => Forbid(),
                    _ => BadRequest(result.Error)
                };
        }

        [HttpPost("schedules/reviews")]
        [ProducesResponseType(typeof(PerformanceReviewResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SubmitPerformanceReview(
            [FromBody] PerformanceReviewRequest request,
            CancellationToken ct = default)
        {
            var result = await sender.Send(new SubmitPerformanceReviewCommand(
                CoordinatorId: CoordinatorId,
                ExternalEventId: request.eventId,
                ExternalScheduleId: request.scheduleId,
                UsherId: request.UsherId,
                Grooming: request.Grooming,
                Professionalism: request.Professionalism,
                Communication: request.Communication,
                Teamwork: request.Teamwork,
                OverallScore: request.OverallScore,
                Remarks: request.Remarks), ct);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetPerformanceReviews),
                    new { request.eventId, request.scheduleId }, result.Value)
                : result.Error.Code switch
                {
                    "REVIEW_001" => Conflict(result.Error),
                    "REVIEW_003" => BadRequest(result.Error),
                    "REVIEW_004" => BadRequest(result.Error),
                    "REVIEW_005" => Forbid(),
                    _ => BadRequest(result.Error)
                };
        }
        [HttpGet("{usherId}/detail")]
        [ProducesResponseType(typeof(UsherDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUsherDetail(
            Guid usherId,
            CancellationToken ct)
        {
            var result = await sender.Send(
                new UsherDetailQuery(usherId), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.Code switch
                {
                    "USHER_004" => NotFound(result.Error),
                    _ => BadRequest(result.Error)
                };
        }
        [HttpGet("{usherId}/events")]
        [ProducesResponseType(typeof(UsherEventHistoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUsherEventHistory(
       Guid usherId,
       CancellationToken ct)
        {
            var result = await sender.Send(
                new UsherEventHistoryQuery(usherId), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.Code switch
                {
                    "USHER_004" => NotFound(result.Error),
                    _ => BadRequest(result.Error)
                };
        }



    }
}