using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UMS.Application.Features.Notification.Command;
using UMS.Application.Features.Notification.Query;
using UMS.Contracts.Notification;
using UMS.Domain.Entities;

namespace UMS.api.Controllers
{

    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    [Produces("application/json")]
    public sealed class NotificationsController(ISender sender) : ControllerBase
    {
        private Guid UserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


        [HttpGet]
        [ProducesResponseType(typeof(PagedNotificationResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] bool? isRead = null,
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            CancellationToken ct = default)
        {
            var result = await sender.Send(
                new NotificationsQuery(UserId, isRead, page, size), ct);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }


        [HttpPut("read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAsRead(
            [FromQuery] Guid? notificationId = null,
            CancellationToken ct = default)
        {
            var result = await sender.Send(
                new MarkNotificationsCommand(UserId, notificationId), ct);

            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }
    }
}