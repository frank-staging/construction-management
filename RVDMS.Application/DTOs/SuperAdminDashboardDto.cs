using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Application.DTOs
{
    public class SuperAdminDashboardDto
    {
        // Overview Statistics
        public OverviewStatsDto Overview { get; set; } = new();

        // Project Statistics
        public ProjectStatsDto Projects { get; set; } = new();

        // User Statistics
        public UserStatsDto Users { get; set; } = new();

        // Regional Breakdown
        public List<RegionalStatsDto> RegionalStats { get; set; } = new();

        // Performance Metrics
        public PerformanceMetricsDto Performance { get; set; } = new();

        // Recent Projects (instead of audit logs)
        public List<RecentProjectDto> RecentProjects { get; set; } = new();

        // Recent Users
        public List<RecentUserDto> RecentUsers { get; set; } = new();
    }

    public class OverviewStatsDto
    {
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int DelayedProjects { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalCounties { get; set; }
        public decimal OverallProgress { get; set; }
    }

    public class ProjectStatsDto
    {
        public decimal AverageProgress { get; set; }
        public int AtRiskProjects { get; set; }
        public int OnTimeProjects { get; set; }
        public int AheadProjects { get; set; }
        public int SlowProjects { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal SpentBudget { get; set; }
        public decimal RemainingBudget { get; set; }
    }

    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int RegionalLeads { get; set; }
        public int CountyDirectors { get; set; }
        public int TechnicalLeads { get; set; }
        public int ClusterSupervisors { get; set; }
        public int ClerksOfWorks { get; set; }
        public int NewThisMonth { get; set; }
        public int ActiveToday { get; set; }
    }

    public class RegionalStatsDto
    {
        public string CountyName { get; set; } = string.Empty;
        public int ProjectCount { get; set; }
        public decimal AverageProgress { get; set; }
        public int DelayedProjects { get; set; }
        public int ActiveUsers { get; set; }
    }

    public class PerformanceMetricsDto
    {
        public decimal OnTimeDeliveryRate { get; set; }
        public decimal BudgetAdherenceRate { get; set; }
        public decimal ReportSubmissionRate { get; set; }
        public List<MonthlyProgressDto> MonthlyProgress { get; set; } = new();
    }

    public class MonthlyProgressDto
    {
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int ProjectsUpdated { get; set; }
    }

    public class RecentProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public decimal Progress { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class RecentUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
