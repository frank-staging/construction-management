using MediatR;
using Microsoft.Extensions.Logging;
using RVDMS.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Application.commands.Projects
{
    public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, bool>
    {
        private readonly IProjectRepository _repository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteProjectCommandHandler> _logger;

        public DeleteProjectCommandHandler(
            IProjectRepository repository,
            ICurrentUserService currentUserService,
            ILogger<DeleteProjectCommandHandler> logger)
        {
            _repository = repository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var project = await _repository.GetByIdAsync(request.Id, cancellationToken);

                if (project == null || project.IsDeleted)
                    return false;

                // Soft delete
                project.IsDeleted = true;
                project.DeletedAt = DateTime.UtcNow;
                project.DeletedBy = _currentUserService.UserId!;
                project.UpdatedAt = DateTime.UtcNow;
                project.UpdatedBy = _currentUserService.UserId!;

                await _repository.UpdateAsync(project, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Project {ProjectId} was soft deleted", request.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project {ProjectId}", request.Id);
                throw;
            }
        }
    }
}
