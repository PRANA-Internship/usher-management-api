using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UMS.Application.Features.Auth.Commands.CreateUser;
using UMS.Application.Features.Auth.Commands.Login;
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
                request.Password);

            var id = await sender.Send(command, ct);
            return CreatedAtAction(nameof(CreateAdmin), new { id }, new { id });
        }


    }
}
