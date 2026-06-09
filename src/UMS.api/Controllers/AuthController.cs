using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using UMS.Application.Features.Auth.Commands.CreateUser;
using UMS.Application.Features.Auth.Commands.ForgotPassword;
using UMS.Application.Features.Auth.Commands.Login;
using UMS.Application.Features.Auth.Commands.RefreshToken;
using UMS.Application.Features.Auth.Commands.ResetPassword;
using UMS.Contracts.Auth;
using UMS.Domain.Entities;

namespace UMS.api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class AuthController(ISender sender) : ControllerBase
    {

        [HttpPost("login")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(LoginRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken ct)
        {
            var command = new LoginCommand(request.Email, request.Password);
            var response = await sender.Send(command, ct);
            return Ok(response);
        }
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh(
    [FromBody] RefreshTokenRequest request,
    CancellationToken ct)
        {
            var result = await sender.Send(
                new RefreshTokenCommand(request.RefreshToken), ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : Unauthorized(result.Error);
        }
        //the below code is for seediing purpose only 
        //will be removed in production 

        [HttpPost("User/seed")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateAdmin(
            [FromBody] CreateUserRequest request,
            CancellationToken ct)
        {
            var command = new CreateUserCommand(
                request.FullName,
                request.Email,
                request.Phone,
                request.Password,
                request.Role);

            var id = await sender.Send(command, ct);
            return CreatedAtAction(nameof(CreateAdmin), new { id }, new { id });
        }
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword(
                    [FromBody] ForgotPasswordRequest request,
                   CancellationToken ct)
        {
            var result = await sender.Send(
                new ForgotPasswordCommand(request.Email), ct);

            return Ok(new { message = "If an account with that email exists, a reset link has been sent." });
        }

        [HttpPut("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest request,
            CancellationToken ct)
        {
            var result = await sender.Send(
                new ResetPasswordCommand(request.Token, request.Password, request.ConfirmPassword), ct);

            return result.IsSuccess
                ? Ok(new { message = "Password reset successfully. You can now log in." })
                : result.Error.Code switch
                {
                    "AUTH_008" => BadRequest(result.Error),
                    "AUTH_009" => Conflict(result.Error),
                    _ => BadRequest(result.Error)
                };
        }

    }
}
