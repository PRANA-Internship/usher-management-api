using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UMS.Application.Features.Events.Queries.GetPublicEvents;
using UMS.Contracts.Events;

namespace UMS.api.Controllers
{
    [ApiController]
    [Route("api/public/events")]
    [Produces("application/json")]
    [AllowAnonymous]
    public sealed class PublicEventsController(ISender sender) : ControllerBase
    {

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<PublicEventResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetPublicEvents(CancellationToken ct)
        {
            var result = await sender.Send(new PublicEventsQuery(), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : StatusCode(StatusCodes.Status502BadGateway, result.Error);
        }
    }
}