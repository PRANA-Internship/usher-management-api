using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UMS.Application.Features.Events.Commands.RemoveCoordinator;
using UMS.Domain.Common;
namespace UMS.api.Controllers
{
    public record UnassignCoordinatorRequest(string eventId, string scheduleId);

    [ApiController]
    [Route("api/events/schedules")]
    [Authorize(Roles = "ADMIN")]
    public class SchedulesController(ISender sender) : ControllerBase
    {
        [HttpDelete("remove")]
        public async Task<IActionResult> UnassignCoordinator([FromBody] UnassignCoordinatorRequest request, CancellationToken ct)
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