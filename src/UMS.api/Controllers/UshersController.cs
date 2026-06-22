using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UMS.Application.Features.Auth.Commands.ApproveApplication;
using UMS.Application.Features.Auth.Commands.SetPassword;
using UMS.Application.Features.Auth.Commands.SubmitApplication;
using UMS.Application.Features.Ushers.Command;
using UMS.Application.Features.Ushers.Command.ApplyToSchedule;
using UMS.Application.Features.Ushers.Command.RespondToInvitaion;
using UMS.Application.Common.Interfaces;
using UMS.Application.Features.Ushers.Queries.GetApplications;
using UMS.Application.Features.Ushers.Queries.GetApplicationsDetail;
using UMS.Application.Features.Ushers.Queries.GetConfirmedApplication;
using UMS.Application.Features.Ushers.Queries.GetMyPendingDecline;
using UMS.Application.Features.Ushers.Queries.GetMyProfile;
using UMS.Contracts.Usher;
using UMS.Domain.Enums;

namespace UMS.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class UshersController(ISender sender, IUsherRepository usherRepository) : ControllerBase
    {
  private readonly IUsherRepository _usherRepository = usherRepository;
        [HttpPost("apply")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(SubmitUsherApplicationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [RequestSizeLimit(20 * 1024 * 1024)] // 20MB total request limit
        public async Task<IActionResult> Apply(
            [FromForm] SubmitUsherApplicationRequest request,
            CancellationToken ct)
        {
            var command = new SubmitUsherApplicationCommand
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address,
                City = request.City,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactPhone = request.EmergencyContactPhone,
                EducationLevel = request.EducationLevel,
                ExperienceSummary = request.ExperienceSummary,
                Languages = request.Languages,
                Sector = request.Sector,
                ProfilePhoto = request.ProfilePhoto,
                IdDocument = request.IdDocument,
                ExternalEventId = request.ExternalEventId,
                ExternalScheduleId = request.ExternalScheduleId
            };

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? CreatedAtAction(nameof(Apply), new { id = result.Value!.FullName }, result.Value)
                : result.Error.Code switch
                {
                    "USHER_001" => Conflict(result.Error),
                    "USHER_002" => BadRequest(result.Error),
                    "USHER_003" => StatusCode(StatusCodes.Status500InternalServerError, result.Error),
                    _ => BadRequest(result.Error)
                };
        }
        [HttpPost("set-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetPassword(
        [FromBody] SetPasswordRequest request,
    CancellationToken ct)
        {
            var result = await sender.Send(
                new SetPasswordCommand(request.Token, request.Password, request.ConfirmPassword), ct);

            return result.IsSuccess
                ? Ok(new { message = "Password set successfully. You can now log in." })
                : result.Error.Code switch
                {
                    "USHER_006" => BadRequest(result.Error),
                    "USHER_007" => Conflict(result.Error),
                    _ => BadRequest(result.Error)
                };
        }
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(GetUsherApplicationDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile(CancellationToken ct)
        {
            //userId from JWT claims
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await sender.Send(new GetMyProfileQuery(userId), ct);

            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }
        [HttpPatch("update-profile")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(
    [FromForm] UpdateUsherProfileRequest request,
    CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var command = new UpdateUsherProfileCommand
            {
                UserId = userId,
                Phone = request.Phone,
                Address = request.Address,
                City = request.City,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactPhone = request.EmergencyContactPhone,
                EducationLevel = request.EducationLevel,
                ExperienceSummary = request.ExperienceSummary,
                Languages = request.Languages,
                Sector = request.Sector,
                ProfilePhoto = request.ProfilePhoto
            };

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Ok(new { message = "Profile updated successfully." })
                : result.Error.Code switch
                {
                    "USHER_004" => NotFound(result.Error),
                    "USHER_002" => BadRequest(result.Error),
                    _ => BadRequest(result.Error)
                };
        }
        [HttpPost("schedules/apply")]
        [Authorize]
        [ProducesResponseType(typeof(ApplyToScheduleResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ApplyToSchedule(
                [FromBody] ApplyToScheduleRequest request,
                CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

           var usher = await _usherRepository.GetByUserIdAsync(userId, ct);
                if (usher is null) return NotFound();
                var result = await sender.Send(new ApplyToScheduleCommand(
                UsherId: usher.Id,
                ExternalEventId: request.ExternalEventId,
                ExternalScheduleId: request.ExternalScheduleId), ct);
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
        [HttpPost("invitations/respond")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RespondToInvitation(
               [FromBody] RespondToInvitationRequest request,
               CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

             var usher = await _usherRepository.GetByUserIdAsync(userId, ct);
                if (usher is null) return NotFound();
                var result = await sender.Send(new RespondToInvitationCommand(
                UsherId: usher.Id,
                InvitationId: request.invitationId,
                Accept: request.Accept), ct);

            return result.IsSuccess
                ? Ok(new { message = request.Accept ? "Invitation accepted." : "Invitation declined." })
                : result.Error.Code switch
                {
                    "USHER_SCH_007" => NotFound(result.Error),
                    "USHER_SCH_008" => StatusCode(StatusCodes.Status403Forbidden, result.Error),
                    "USHER_SCH_009" => Conflict(result.Error),
                    "USHER_SCH_004" => Conflict(result.Error),
                    _ => BadRequest(result.Error)
                };
        }
        [HttpGet("schedules/me")]
        [Authorize]
        [ProducesResponseType(typeof(PagedScheduleResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMySchedules(
            [FromQuery] MyScheduleStatusFilter filter,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            CancellationToken ct = default)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await sender.Send(
                new GetMyPendingDeclinedQuery(userId, filter, page, size), ct);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
        [HttpGet("schedules/confirmed")]
        [Authorize]
        [ProducesResponseType(typeof(PagedConfirmedScheduleResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyConfirmedSchedules(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            CancellationToken ct = default)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await sender.Send(
                new GetMyConfirmedSchedulesQuery(userId, page, size), ct);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }


    }
}