using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RVDMS.Application.commands.Projects;
using RVDMS.Application.Common;
using RVDMS.Application.DTOs;
using RVDMS.Application.Queries.Projects;

namespace RVDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get a paged list of projects based on filters.
        /// </summary>
        
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<ProjectDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetProjects([FromQuery] ProjectFilter filter, CancellationToken cancellationToken)
        {
            var query = new GetAllProjectsQuery(filter);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get a single project by its Id.
        /// </summary>
        
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProjectDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProjectById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var query = new GetProjectByIdQuery(id);
            var project = await _mediator.Send(query, cancellationToken);

            if (project == null)
                return NotFound($"Project with Id {id} was not found.");

            return Ok(project);
        }

        [HttpGet("my-project")]
        [ProducesResponseType(typeof(ProjectDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMyProject(CancellationToken cancellationToken)
        {
            var query = new GetMyProjectQuery();
            var project = await _mediator.Send(query, cancellationToken);

            if (project == null)
                return NotFound("No project assigned to you");

            return Ok(project);
        }
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,RegionalLead,CountyDirector")]
        [ProducesResponseType(typeof(ProjectDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto createDto, CancellationToken cancellationToken)
        {
            var command = new CreateProjectCommand(createDto);
            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetProjectById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,RegionalLead,CountyDirector")]
        [ProducesResponseType(typeof(ProjectDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProject([FromRoute] Guid id, [FromBody] UpdateProjectDto updateDto, CancellationToken cancellationToken)
        {
            var command = new UpdateProjectCommand(id, updateDto);
            var result = await _mediator.Send(command, cancellationToken);

            if (result == null)
                return NotFound($"Project with Id {id} was not found.");

            return Ok(result);
        }

        /// <summary>
        /// Soft delete a project (mark as deleted).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,RegionalLead")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProject([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteProjectCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
                return NotFound($"Project with Id {id} was not found.");

            return NoContent();
        }
    }
}
