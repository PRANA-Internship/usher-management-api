using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UMS.Application.Features.Account.Commands.UpdateProfile;
using UMS.Application.Features.Admin.Queries;
using UMS.Application.Features.Auth.Commands.ApproveApplication;
using UMS.Application.Features.Auth.Commands.RejectApplication;
using UMS.Application.Features.Coordinator.Queries.GetMyProfile;
using UMS.Application.Features.Ushers.Queries.GetApplications;
using UMS.Application.Features.Ushers.Queries.GetApplicationsDetail;
using UMS.Contracts.Admin;
using UMS.Contracts.Coordinator;
using UMS.Contracts.User;
using UMS.Contracts.Usher;
using UMS.Domain.Enums;
namespace UMS.api.Controllers
{
    [ApiController]
    [Route("api/admin/ushers")]
    [Produces("application/json")]
    [Authorize(Roles = "ADMIN")]
    public sealed class AdminController(ISender sender) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(GetUsherApplicationsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetApplications(
               [FromQuery] int page = 1,
               [FromQuery] int size = 10,
               [FromQuery] ApprovalStatus? status = null,
               [FromQuery] string? searchName = null,
               CancellationToken ct = default)
        {
            var result = await sender.Send(
                new GetUsherApplicationsQuery(page, size, status, searchName), ct);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
        [HttpGet("me")]
        [ProducesResponseType(typeof(GetMyProfileCoordinator), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile(
           CancellationToken ct)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await sender.Send(
                new GetMyProfileQuery(adminId), ct);

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
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await sender.Send(new UpdateUserProfileCommand(
                UserId: adminId,
                FullName: request.FullName,
                Phone: request.Phone), ct);
            return result.IsSuccess
                ? Ok(new { message = "Profile updated successfully." })
                : result.Error.Code switch
                {
                    "USER_NOT_FOUND" => NotFound(result.Error),
                    "AUTH_010" => BadRequest(result.Error),
                    _ => BadRequest(result.Error)
                };
        }

        [HttpGet("{usherId:guid}")]
        [ProducesResponseType(typeof(GetUsherApplicationDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetail(Guid usherId, CancellationToken ct)
        {
            var result = await sender.Send(
                new GetUsherApplicationDetailQuery(usherId), ct);

            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }
        [HttpPost("{usherId:guid}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Approve(Guid usherId, CancellationToken ct)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await sender.Send(
                new ApproveUsherApplicationCommand(adminId, usherId), ct);

            return result.IsSuccess
                ? Ok(new { message = "Application approved. Password setup email sent." })
                : result.Error.Code switch
                {
                    "USHER_004" => NotFound(result.Error),
                    "USHER_005" => Conflict(result.Error),
                    _ => BadRequest(result.Error)
                };
        }

        [HttpPost("{usherId:guid}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Reject(Guid usherId, CancellationToken ct)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await sender.Send(
                new RejectUsherApplicationCommand(adminId, usherId), ct);

            return result.IsSuccess
                ? Ok(new { message = "Application rejected. Notification email sent." })
                : result.Error.Code switch
                {
                    "USHER_004" => NotFound(result.Error),
                    "USHER_008" => Conflict(result.Error),
                    "USHER_009" => Conflict(result.Error),
                    _ => BadRequest(result.Error)
                };
        }
        [HttpGet("coordinators")]
        [ProducesResponseType(typeof(GetCoordinatorsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCoordinators(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? searchName = null,
        CancellationToken ct = default)
        {
            var result = await sender.Send(
                new GetCoordinatorsQuery(page, size, searchName), ct);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

    }
}