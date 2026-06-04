using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RVDMS.Application.commands.Users.AssignRole;
using RVDMS.Application.commands.Users.AuthToken;
using RVDMS.Application.commands.Users.Login;
using RVDMS.Application.commands.Users.PasswordChange;
using RVDMS.Application.commands.Users.Register;
using RVDMS.Application.Common;
using RVDMS.Application.DTOs;

namespace RVDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<AuthResponseDto>>> Register(
            [FromBody] RegisterUserCommand command,
            CancellationToken cancellationToken)
        {
            
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<AuthResponseDto>>> Login(
            [FromBody] LoginUserCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken(
    [FromBody] RefreshTokenCommand command,
    CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
    [FromBody] ChangePasswordCommand command,
    CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new LogoutCommand(), cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPost("assign-role")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result.Error);
            return Ok(new { message = "Role assigned successfully" });
        }
    }
}
