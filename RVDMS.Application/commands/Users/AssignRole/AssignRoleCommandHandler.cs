using MediatR;
using Microsoft.AspNetCore.Identity;
using RVDMS.Application.Common;
using RVDMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Application.commands.Users.AssignRole
{
    public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AssignRoleCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<bool>> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            // Get current roles and remove them
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Add new role
            var result = await _userManager.AddToRoleAsync(user, request.Role);
            if (!result.Succeeded)
                return Result<bool>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Result<bool>.Success(true);
        }
    }
}
