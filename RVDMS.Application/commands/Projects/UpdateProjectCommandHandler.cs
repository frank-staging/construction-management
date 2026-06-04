using MediatR;
using RVDMS.Application.Interfaces;
using RVDMS.Application.Queries.Projects;
using RVDMS.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Application.commands.Projects
{
    public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
    {
        private readonly IProjectRepository _repository;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProjectCommandHandler(
            IProjectRepository repository,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _currentUserService = currentUserService;
        }

        public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {request.Id} not found");

            // Update basic info
            project.Name = request.ProjectData.Name;
            project.TenderNumber = request.ProjectData.TenderNumber;
            project.ProjectLeadId = request.ProjectData.ProjectLeadId;
            project.ContractorName = request.ProjectData.ContractorName;
            project.ConsultantName = request.ProjectData.ConsultantName;
            project.Description = request.ProjectData.Description;
            project.ContractSum = request.ProjectData.ContractSum;
            project.StartDate = request.ProjectData.StartDate;
            project.EndDate = request.ProjectData.EndDate;
            project.Status = Enum.Parse<ProjectStatus>(request.ProjectData.Status);

            // Update location
            project.Latitude = request.ProjectData.Latitude;
            project.Longitude = request.ProjectData.Longitude;
            project.RadiusInMeters = request.ProjectData.RadiusInMeters;

            // Update relationships
            project.WardId = request.ProjectData.WardId;
            project.ClusterId = request.ProjectData.ClusterId;

            // Audit
            project.UpdatedAt = DateTime.UtcNow;
            project.UpdatedBy = _currentUserService.UserId!;

            await _repository.UpdateAsync(project, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            // Return updated project (you'll need a GetProjectById query)
            // Or use AutoMapper
            return new ProjectDto { Id = project.Id, Name = project.Name }; // Simplified
        }
    }
}
