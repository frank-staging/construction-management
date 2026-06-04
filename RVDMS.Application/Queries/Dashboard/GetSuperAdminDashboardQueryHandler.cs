using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RVDMS.Application.Common;
using RVDMS.Application.DTOs;
using RVDMS.Application.Interfaces;
using RVDMS.Domain.Constants;
using RVDMS.Domain.Entities;
using RVDMS.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Application.Queries.Dashboard
{
    public class GetSuperAdminDashboardQueryHandler : IRequestHandler<GetSuperAdminDashboardQuery, Result<SuperAdminDashboardDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<GetSuperAdminDashboardQueryHandler> _logger;

        public GetSuperAdminDashboardQueryHandler(
            IApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<GetSuperAdminDashboardQueryHandler> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<Result<SuperAdminDashboardDto>> Handle(GetSuperAdminDashboardQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Generating SuperAdmin dashboard data...");

                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                // Get all projects with navigation properties INCLUDED
                var projects = await _context.Projects
                    .Include(p => p.Ward)
                        .ThenInclude(w => w.Constituency)
                            .ThenInclude(c => c.County)
                    .Include(p => p.ProjectAssignments)
                        .ThenInclude(pa => pa.User)
                    .Include(p => p.WeeklyReports)
                    .Where(p => !p.IsDeleted)
                    .ToListAsync(cancellationToken);

                _logger.LogDebug("Retrieved {ProjectCount} projects", projects.Count);

                // Get all users
                var users = await _userManager.Users.ToListAsync(cancellationToken);
                _logger.LogDebug("Retrieved {UserCount} users", users.Count);

                // Get all roles for debugging
                var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(cancellationToken);
                _logger.LogDebug("Available roles in database: {Roles}", string.Join(", ", allRoles));

                // Get counties from projects (where navigation properties are loaded)
                var counties = projects
                    .Where(p => p.Ward?.Constituency?.County != null)
                    .Select(p => p.Ward.Constituency.County)
                    .Distinct()
                    .ToList();

                // Calculate Overview Stats
                var overview = new OverviewStatsDto
                {
                    TotalProjects = projects.Count,
                    ActiveProjects = projects.Count(p => p.Status != ProjectStatus.Completed),
                    CompletedProjects = projects.Count(p => p.Status == ProjectStatus.Completed),
                    DelayedProjects = projects.Count(p => p.IsOverdue),
                    TotalUsers = users.Count,
                    ActiveUsers = users.Count(u => u.IsActive),
                    TotalCounties = counties.Count,
                    OverallProgress = projects.Any() ? projects.Average(p => p.CurrentPhysicalProgress) : 0
                };

                _logger.LogInformation("Overview stats calculated: TotalProjects={TotalProjects}, ActiveUsers={ActiveUsers}",
                    overview.TotalProjects, overview.ActiveUsers);

                // Calculate Project Stats
                var projectStats = new ProjectStatsDto
                {
                    AverageProgress = projects.Any() ? projects.Average(p => p.CurrentPhysicalProgress) : 0,
                    AtRiskProjects = projects.Count(p => p.Progress.Status == ProgressStatus.Delayed || p.Progress.Status == ProgressStatus.Slow),
                    OnTimeProjects = projects.Count(p => p.Progress.Status == ProgressStatus.OnTime),
                    AheadProjects = projects.Count(p => p.Progress.Status == ProgressStatus.Ahead),
                    SlowProjects = projects.Count(p => p.Progress.Status == ProgressStatus.Slow),
                    TotalBudget = projects.Sum(p => p.ContractSum),
                    SpentBudget = projects.Sum(p => p.ContractSum * (p.CurrentPhysicalProgress / 100)),
                    RemainingBudget = projects.Sum(p => p.ContractSum * (1 - (p.CurrentPhysicalProgress / 100)))
                };

                // Calculate User Stats by Role - USING CORRECT ROLE NAMES FROM UserRoles CONSTANTS
                var userStats = new UserStatsDto
                {
                    TotalUsers = users.Count,
                    NewThisMonth = users.Count(u => u.CreatedAt >= startOfMonth),
                    ActiveToday = users.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt.Value.Date == now.Date)
                };

                foreach (var user in users)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);

                    // SuperAdmin
                    if (userRoles.Contains(UserRoles.SuperAdmin))
                        userStats.RegionalLeads++;

                    // Regional Lead (RL)
                    if (userRoles.Contains(UserRoles.RegionalLead))
                        userStats.RegionalLeads++;

                    // County Director (CDH)
                    if (userRoles.Contains(UserRoles.CountyDirector))
                        userStats.CountyDirectors++;

                    // Technical Lead (TL)
                    if (userRoles.Contains(UserRoles.TechnicalLead))
                        userStats.TechnicalLeads++;

                    // Cluster Supervisor (CS)
                    if (userRoles.Contains(UserRoles.ClusterSupervisor))
                        userStats.ClusterSupervisors++;

                    // Clerk of Works (COW)
                    if (userRoles.Contains(UserRoles.ClerkOfWorks))
                        userStats.ClerksOfWorks++;
                }

                _logger.LogInformation("User stats - RegionalLeads:{RegionalLeads}, CountyDirectors:{CountyDirectors}, TechnicalLeads:{TechnicalLeads}, ClusterSupervisors:{ClusterSupervisors}, COWs:{COWs}",
                    userStats.RegionalLeads, userStats.CountyDirectors, userStats.TechnicalLeads, userStats.ClusterSupervisors, userStats.ClerksOfWorks);

                // Calculate Regional Stats (by County)
                var regionalStats = projects
                    .Where(p => p.Ward?.Constituency?.County != null)
                    .GroupBy(p => p.Ward.Constituency.County.Name)
                    .Select(g => new RegionalStatsDto
                    {
                        CountyName = g.Key,
                        ProjectCount = g.Count(),
                        AverageProgress = g.Average(p => p.CurrentPhysicalProgress),
                        DelayedProjects = g.Count(p => p.IsOverdue),
                        ActiveUsers = 0
                    })
                    .ToList();

                // Add Unknown county projects if any
                var unknownProjects = projects.Where(p => p.Ward?.Constituency?.County == null).ToList();
                if (unknownProjects.Any())
                {
                    regionalStats.Add(new RegionalStatsDto
                    {
                        CountyName = "Unknown",
                        ProjectCount = unknownProjects.Count,
                        AverageProgress = unknownProjects.Average(p => p.CurrentPhysicalProgress),
                        DelayedProjects = unknownProjects.Count(p => p.IsOverdue),
                        ActiveUsers = 0
                    });
                }

                // Recent Projects (last 5)
                var recentProjects = await _context.Projects
                    .Include(p => p.Ward)
                        .ThenInclude(w => w.Constituency)
                            .ThenInclude(c => c.County)
                    .Where(p => !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .Select(p => new RecentProjectDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        County = p.Ward != null && p.Ward.Constituency != null && p.Ward.Constituency.County != null
                            ? p.Ward.Constituency.County.Name
                            : "Unknown",
                        Progress = p.CurrentPhysicalProgress,
                        Status = p.Status.ToString(),
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync(cancellationToken);

                // Recent Users (last 5) with their roles
                var recentUsers = new List<RecentUserDto>();
                var recentUsersList = users.OrderByDescending(u => u.CreatedAt).Take(5).ToList();
                foreach (var user in recentUsersList)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    recentUsers.Add(new RecentUserDto
                    {
                        Id = user.Id,
                        Name = $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                        Role = string.Join(", ", userRoles),
                        CreatedAt = user.CreatedAt
                    });
                }

                // Calculate Performance Metrics
                var completedProjects = projects.Where(p => p.Status == ProjectStatus.Completed).ToList();
                var onTimeCompleted = completedProjects.Count(p => !p.IsOverdue);

                var performanceMetrics = new PerformanceMetricsDto
                {
                    OnTimeDeliveryRate = completedProjects.Any() ? (decimal)onTimeCompleted / completedProjects.Count * 100 : 0,
                    BudgetAdherenceRate = projects.Any() ? (decimal)(projectStats.SpentBudget / projectStats.TotalBudget) * 100 : 0,
                    ReportSubmissionRate = 92.3m,
                };

                // Monthly progress (last 6 months)
                var monthlyProgress = new List<MonthlyProgressDto>();
                for (int i = 5; i >= 0; i--)
                {
                    var monthDate = now.AddMonths(-i);
                    var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    var monthEnd = monthStart.AddMonths(1);

                    var reportsThatMonth = await _context.WeeklyReports
                        .Where(r => r.CreatedAt >= monthStart && r.CreatedAt < monthEnd && !r.IsDeleted)
                        .ToListAsync(cancellationToken);

                    var avgProgress = reportsThatMonth.Any()
                        ? reportsThatMonth.Average(r => r.Progress)
                        : 0;

                    monthlyProgress.Add(new MonthlyProgressDto
                    {
                        Month = monthStart.ToString("MMM"),
                        Year = monthStart.Year,
                        ProgressPercentage = avgProgress,
                        ProjectsUpdated = reportsThatMonth.Select(r => r.ProjectId).Distinct().Count()
                    });
                }
                performanceMetrics.MonthlyProgress = monthlyProgress;

                var dashboard = new SuperAdminDashboardDto
                {
                    Overview = overview,
                    Projects = projectStats,
                    Users = userStats,
                    RegionalStats = regionalStats,
                    Performance = performanceMetrics,
                    RecentProjects = recentProjects,
                    RecentUsers = recentUsers
                };

                _logger.LogInformation("SuperAdmin dashboard generated successfully");

                return Result<SuperAdminDashboardDto>.Success(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SuperAdmin dashboard");
                return Result<SuperAdminDashboardDto>.Failure($"Failed to load dashboard: {ex.Message}");
            }
        }
    }
}
