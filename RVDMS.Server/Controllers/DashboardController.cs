using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RVDMS.Application.Common;
using RVDMS.Application.DTOs;
using RVDMS.Application.Queries.Dashboard;

namespace RVDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get SuperAdmin Dashboard Data
        /// </summary>
        [HttpGet("superadmin")]
        //[Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(Result<SuperAdminDashboardDto>), 200)]
        public async Task<IActionResult> GetSuperAdminDashboard(CancellationToken cancellationToken)
        {
            var query = new GetSuperAdminDashboardQuery();
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
    }
}
