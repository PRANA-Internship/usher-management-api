using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UMS.Application.Features.Auth.Commands.ApproveApplication;
using UMS.Application.Features.Auth.Commands.SubmitApplication;
using UMS.Application.Features.Ushers.Queries.GetApplications;
using UMS.Application.Features.Ushers.Queries.GetApplicationsDetail;
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
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(GetUsherApplicationsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetApplications(
                [FromQuery] int page = 1,
                [FromQuery] int size = 10,
                [FromQuery] ApprovalStatus? status = null,
                CancellationToken ct = default)
        {
            var result = await sender.Send(
                new GetUsherApplicationsQuery(page, size, status), ct);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
        [HttpGet("{usherId:guid}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(GetUsherApplicationDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetail(Guid usherId, CancellationToken ct)
        {
            var result = await sender.Send(
                new GetUsherApplicationDetailQuery(usherId), ct);

            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }
        [HttpPost("{usherId:guid}/approve")]
        [Authorize(Roles = "ADMIN")]
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
    }
}
