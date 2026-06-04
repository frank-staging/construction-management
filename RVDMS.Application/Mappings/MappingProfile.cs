using AutoMapper;
using RVDMS.Application.DTOs;
using RVDMS.Application.Queries.Projects;
using RVDMS.Domain.Entities;
using RVDMS.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RVDMS.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Create Project -> ProjectDto mapping
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.ProjectLeadName,
                    opt => opt.MapFrom(src => src.ProjectLead != null
                        ? $"{src.ProjectLead.FirstName} {src.ProjectLead.LastName}"
                        : null))
                .ForMember(dest => dest.County,
                    opt => opt.MapFrom(src => src.Ward != null && src.Ward.Constituency != null && src.Ward.Constituency.County != null
                        ? src.Ward.Constituency.County.Name
                        : string.Empty))
                .ForMember(dest => dest.Constituency,
                    opt => opt.MapFrom(src => src.Ward != null && src.Ward.Constituency != null
                        ? src.Ward.Constituency.Name
                        : string.Empty))
                .ForMember(dest => dest.Ward,
                    opt => opt.MapFrom(src => src.Ward != null
                        ? src.Ward.Name
                        : string.Empty))
                .ForMember(dest => dest.Cluster,
                    opt => opt.MapFrom(src => src.Cluster != null
                        ? src.Cluster.Name
                        : null))
                .ForMember(dest => dest.TimeElapsedPercentage,
                    opt => opt.MapFrom(src => Math.Round(src.TimeElapsedPercentage, 2)))
                .ForMember(dest => dest.Variance,
                    opt => opt.MapFrom(src => Math.Round(src.Progress.Variance, 2)))
                .ForMember(dest => dest.ProgressStatus,
                    opt => opt.MapFrom(src => src.Progress.Status.ToString()))
                .ForMember(dest => dest.ProgressStatusColor,
                    opt => opt.MapFrom(src => src.Progress.GetStatusColor()))
                .ForMember(dest => dest.IsAtRisk,
                    opt => opt.MapFrom(src => src.Progress.Status == ProgressStatus.Delayed || src.Progress.Status == ProgressStatus.Slow))
                .ForMember(dest => dest.DaysRemaining,
                    opt => opt.MapFrom(src => Math.Max(0, (src.EndDate - DateTime.UtcNow).Days)))
                .ForMember(dest => dest.ClerkOfWorks,
                    opt => opt.MapFrom(src => src.ProjectAssignments
                        .Where(a => a.Role == "COW" && a.RevokedAt == null)
                        .Select(a => a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : null)
                        .FirstOrDefault()))
                .ForMember(dest => dest.ClusterSupervisors,
                    opt => opt.MapFrom(src => src.ProjectAssignments
                        .Where(a => a.Role == "CS" && a.RevokedAt == null)
                        .Select(a => a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : null)
                        .Where(name => name != null)
                        .ToList()))
                .ForMember(dest => dest.TotalWeeklyReports,
                    opt => opt.MapFrom(src => src.WeeklyReports.Count))
                .ForMember(dest => dest.LastReportDate,
                    opt => opt.MapFrom(src => src.WeeklyReports
                        .OrderByDescending(wr => wr.CreatedAt)
                        .Select(wr => wr.CreatedAt)
                        .FirstOrDefault()))
                .ForMember(dest => dest.HasOverdueReports,
                    opt => opt.MapFrom(src => src.WeeklyReports
                        .OrderByDescending(wr => wr.CreatedAt)
                        .Select(wr => wr.CreatedAt)
                        .FirstOrDefault() < DateTime.UtcNow.AddDays(-7)));

            // CreateProjectDto -> Project mapping
            CreateMap<CreateProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentPhysicalProgress, opt => opt.MapFrom(src => 0m))
                .ForMember(dest => dest.ProjectAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.WeeklyReports, opt => opt.Ignore());

            // UpdateProjectDto -> Project mapping
            CreateMap<UpdateProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.WeeklyReports, opt => opt.Ignore());
        }
    }
}
