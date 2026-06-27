using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UMS.Application.Features.Auth.Commands.ChangePassword;
using UMS.Contracts.Auth;

namespace UMS.api.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public sealed class AccountController(ISender sender) : ControllerBase
{
    [HttpPatch("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await sender.Send(
            new ChangePasswordCommand(
                userId,
                request.CurrentPassword,
                request.NewPassword,
                request.ConfirmNewPassword), ct);

        return result.IsSuccess
            ? Ok(new { message = "Password changed successfully." })
            : result.Error.Code switch
            {
                "AUTH_010" => BadRequest(result.Error),
                "AUTH_011" => BadRequest(result.Error),
                "USER_NOT_FOUND" => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
    }
}