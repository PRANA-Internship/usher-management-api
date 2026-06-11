using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UMS.Application.Features.Staff.Commands.CreateStaff;
using UMS.Application.Features.Staff.Commands.RemoveStaffValidator;
using UMS.Application.Features.Staff.Commands.ResendSetupLink;
using UMS.Application.Features.Staff.Commands.SetUpPassword;
using UMS.Application.Features.Staff.Queries.GetStaff;
using UMS.Contracts.Staff;
using UMS.Contracts.Usher;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.api.Controllers
{

    [ApiController]
    [Route("api/staff")]
    [Produces("application/json")]
    public sealed class StaffController(ISender sender) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(CreateStaffResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateStaff(
            [FromBody] CreateStaffRequest request,
            CancellationToken ct)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await sender.Send(new CreateStaffCommand(
                AdminId: adminId,
                FullName: request.FullName,
                Email: request.Email,
                Phone: request.Phone,
                Role: request.Role), ct);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetStaff), new { id = result.Value!.UserId }, result.Value)
                : result.Error.Code switch
                {
                    "STAFF_001" => Conflict(result.Error),
                    "STAFF_002" => BadRequest(result.Error),
                    _ => BadRequest(result.Error)
                };
        }

        [HttpPost("setup-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SetupPassword(
            [FromBody] SetupPasswordRequest request,
            CancellationToken ct)
        {
            var result = await sender.Send(new SetupPasswordCommand(
                request.Token,
                request.Password,
                request.ConfirmPassword), ct);

            return result.IsSuccess
                ? Ok(new { message = "Password set successfully. You can now log in." })
                : result.Error.Code switch
                {
                    "STAFF_003" => BadRequest(result.Error),
                    "STAFF_004" => Conflict(result.Error),
                    _ => BadRequest(result.Error)
                };
        }
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(GetStaffResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStaff(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] UserRole? role = null,
            [FromQuery] UserStatus? status = null,
            [FromQuery] string? searchName = null,
            CancellationToken ct = default)
        {
            var result = await sender.Send(
                new GetStaffQuery(page, size, role, status, searchName), ct);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
        [HttpPost("resend-setup")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(ResendSetupLinkResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ResendSetupLink(
           [FromBody] ResendSetupLinkRequest request,
           CancellationToken ct)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await sender.Send(
                new ResendSetupLinkCommand(adminId, request.StaffUserId), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.Error.Code switch
                {
                    "STAFF_005" => NotFound(result.Error),
                    "STAFF_007" => Conflict(result.Error),
                    "STAFF_009" => BadRequest(result.Error),
                    "STAFF_010" => StatusCode(StatusCodes.Status403Forbidden, result.Error),
                    _ => BadRequest(result.Error)
                };
        }

        [HttpDelete("remove")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveStaff(
            [FromBody] RemoveStaffRequest request,
            CancellationToken ct)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await sender.Send(
                new RemoveStaffCommand(adminId, request.StaffUserId), ct);

            return result.IsSuccess
                ? Ok(new { message = "Staff member removed successfully." })
                : result.Error.Code switch
                {
                    "STAFF_005" => NotFound(result.Error),
                    "STAFF_008" => BadRequest(result.Error),
                    "STAFF_009" => BadRequest(result.Error),
                    "STAFF_010" => StatusCode(StatusCodes.Status403Forbidden, result.Error),
                    _ => BadRequest(result.Error)
                };
        }
    }
}