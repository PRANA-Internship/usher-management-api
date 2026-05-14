using MediatR;
using Microsoft.AspNetCore.Mvc;
using UMS.Application.Features.Auth.Commands.SubmitApplication;
using UMS.Contracts.Usher;

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
    }
}
