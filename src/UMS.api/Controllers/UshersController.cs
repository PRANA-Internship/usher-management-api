using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UMS.Application.Features.Auth.Commands.ApproveApplication;
using UMS.Application.Features.Auth.Commands.SetPassword;
using UMS.Application.Features.Auth.Commands.SubmitApplication;
using UMS.Application.Features.Ushers.Queries.GetApplications;
using UMS.Application.Features.Ushers.Queries.GetApplicationsDetail;
using UMS.Application.Features.Ushers.Queries.GetMyProfile;
using UMS.Contracts.Usher;
using UMS.Domain.Enums;

namespace UMS.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class UshersController(ISender sender) : ControllerBase
    {

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
                IdDocument = request.IdDocument
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
    }
}
