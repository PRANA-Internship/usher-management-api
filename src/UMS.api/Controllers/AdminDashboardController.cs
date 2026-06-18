using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UMS.Application.Features.Admin.Queries.Dashboard;
using UMS.Contracts.Admin.Dashboard;

namespace UMS.api.Controllers
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = "ADMIN")]
    [Produces("application/json")]
    public sealed class AdminDashboardController(ISender sender) : ControllerBase
    {

        [HttpGet("analytics")]
        [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard(CancellationToken ct)
        {
            var result = await sender.Send(new GetAdminDashboardQuery(), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : StatusCode(StatusCodes.Status500InternalServerError, result.Error);
        }
        [HttpGet("attendance-trend")]
        [ProducesResponseType(typeof(AttendanceTrendResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAttendanceTrend(CancellationToken ct)
        {
            var result = await sender.Send(new AttendanceTrendQuery(), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : StatusCode(StatusCodes.Status500InternalServerError, result.Error);
        }







    }
}