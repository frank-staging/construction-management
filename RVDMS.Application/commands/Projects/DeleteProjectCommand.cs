using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Application.commands.Projects
{
    public record DeleteProjectCommand(Guid Id) : IRequest<bool>;
}
