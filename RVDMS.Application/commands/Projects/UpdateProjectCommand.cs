using MediatR;
using RVDMS.Application.DTOs;
using RVDMS.Application.Queries.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Application.commands.Projects
{
    public record UpdateProjectCommand(Guid Id, UpdateProjectDto ProjectData) : IRequest<ProjectDto>;

}
