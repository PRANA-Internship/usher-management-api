using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UMS.Application.Features.Auth.Commands.ApproveApplication;
using UMS.Application.Features.Ushers.Queries.GetApplications;
using UMS.Application.Features.Ushers.Queries.GetApplicationsDetail;
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
               CancellationToken ct = default)
        {
            var result = await sender.Send(
                new GetUsherApplicationsQuery(page, size, status), ct);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
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
    }
}