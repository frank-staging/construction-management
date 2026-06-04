using AutoMapper;
using MediatR;
using RVDMS.Application.Interfaces;
using RVDMS.Application.Queries.Projects;
using RVDMS.Domain.Entities;
using RVDMS.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Application.commands.Projects
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
    {
        private readonly IProjectRepository _repository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public CreateProjectCommandHandler(
            IProjectRepository repository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _repository = repository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.ProjectData.Name,
                TenderNumber = request.ProjectData.TenderNumber,
                ProjectLeadId = request.ProjectData.ProjectLeadId,
                ContractorName = request.ProjectData.ContractorName,
                ConsultantName = request.ProjectData.ConsultantName,
                Description = request.ProjectData.Description,
                ContractSum = request.ProjectData.ContractSum,
                StartDate = request.ProjectData.StartDate,
                EndDate = request.ProjectData.EndDate,
                Status = Enum.Parse<ProjectStatus>(request.ProjectData.Status),
                Latitude = request.ProjectData.Latitude,
                Longitude = request.ProjectData.Longitude,
                RadiusInMeters = request.ProjectData.RadiusInMeters,
                WardId = request.ProjectData.WardId,
                ClusterId = request.ProjectData.ClusterId,
                CurrentPhysicalProgress = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUserService.UserId!
            };

            var created = await _repository.AddAsync(project, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            //  AutoMapper to map to ProjectDto
            return _mapper.Map<ProjectDto>(created);
        }
    }
}
