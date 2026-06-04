using MediatR;
using RVDMS.Application.Common;
using RVDMS.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Application.Queries.Dashboard
{
    public record GetSuperAdminDashboardQuery() : IRequest<Result<SuperAdminDashboardDto>>;
}
