using MediatR;
using RVDMS.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Application.commands.Users.AssignRole
{
    public record AssignRoleCommand(string UserId, string Role) : IRequest<Result<bool>>;
}
