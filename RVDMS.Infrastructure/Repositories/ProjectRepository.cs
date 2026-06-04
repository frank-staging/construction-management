using Microsoft.EntityFrameworkCore;
using RVDMS.Application.Interfaces;
using RVDMS.Application.Queries.Projects;
using RVDMS.Domain.Entities;
using RVDMS.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IApplicationDbContext _context;

        public ProjectRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            await _context.Projects.AddAsync(project, cancellationToken);
            return project;
        }

        public async Task<int> CountAsync(ProjectFilter filter, CancellationToken cancellationToken = default)
        {
            var query = _context.Projects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = filter.SearchTerm.ToLower();
                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, $"%{search}%") ||
                    EF.Functions.ILike(p.TenderNumber ?? "", $"%{search}%") ||
                    EF.Functions.ILike(p.ContractorName, $"%{search}%") ||
                    EF.Functions.ILike(p.ConsultantName, $"%{search}%"));
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
        {

            return await _context.Projects
                .Include(p => p.Ward)
                    .ThenInclude(w => w.Constituency)
                        .ThenInclude(c => c.County)
                .Include(p => p.Cluster)
                .Include(p => p.ProjectLead)
                .Include(p => p.ProjectAssignments)
                    .ThenInclude(pa => pa.User)
                .Include(p => p.WeeklyReports)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .Include(p => p.Ward)
                    .ThenInclude(w => w.Constituency)
                        .ThenInclude(c => c.County)
                .Include(p => p.Cluster)
                .Include(p => p.ProjectLead)
                .Include(p => p.ProjectAssignments)
                    .ThenInclude(pa => pa.User)
                .Include(p => p.WeeklyReports)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Project>> GetFilteredAsync(ProjectFilter filter, CancellationToken cancellationToken = default)
        {
            var query = _context.Projects
                 .Include(p => p.Ward)
                     .ThenInclude(w => w.Constituency)
                         .ThenInclude(c => c.County)
                 .Include(p => p.Cluster)
                 .Include(p => p.ProjectLead)
                 .Include(p => p.ProjectAssignments)
                     .ThenInclude(pa => pa.User)
                 .Include(p => p.WeeklyReports)
                 .AsNoTracking()
                 .AsQueryable();

            // Global search
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = $"%{filter.SearchTerm}%"; // % = wildcard before and after
                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, search) ||
                    EF.Functions.ILike(p.TenderNumber ?? "", search) ||
                    EF.Functions.ILike(p.ContractorName, search) ||
                    EF.Functions.ILike(p.ConsultantName, search));
            }

            // Filter by progress status (in-memory calculation)
            if (filter.ProgressStatus.HasValue)
            {
                var projects = await query.ToListAsync(cancellationToken);
                query = projects.Where(p => p.Progress.Status == filter.ProgressStatus.Value).AsQueryable();
            }

            // Additional filters
            if (filter.CountyId.HasValue)
                query = query.Where(p => p.Ward.Constituency.County.Id == filter.CountyId.Value);

            if (filter.ConstituencyId.HasValue)
                query = query.Where(p => p.Ward.Constituency.Id == filter.ConstituencyId.Value);

            if (filter.WardId.HasValue)
                query = query.Where(p => p.WardId == filter.WardId.Value);

            if (filter.ClusterId.HasValue)
                query = query.Where(p => p.ClusterId == filter.ClusterId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Role) && !string.IsNullOrWhiteSpace(filter.AssignedToUserId))
                query = query.Where(p => p.ProjectAssignments.Any(pa => pa.Role == filter.Role && pa.UserId == filter.AssignedToUserId));

            // Sorting
            query = query.ApplySorting(filter.SortBy, "Name");

            // Pagination
            query = query.ApplyPagination(filter.PageNumber, filter.PageSize);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<Project?> GetProjectByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            // Find project where this user is the lastUpdatedBy OR has an active assignment
            return await _context.Projects
                .Include(p => p.Ward)
                    .ThenInclude(w => w.Constituency)
                    .ThenInclude(c => c.County)
                .Include(p => p.Cluster)
                .Include(p => p.ProjectLead)
                .Include(p => p.ProjectAssignments)
                    .ThenInclude(pa => pa.User)
                .Include(p => p.WeeklyReports)
                .FirstOrDefaultAsync(p =>
                    p.LastUpdatedById == userId ||
                    p.ProjectAssignments.Any(pa => pa.UserId == userId && pa.RevokedAt == null),
                    cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task SoftDeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
        {
            var project = await GetByIdAsync(id, cancellationToken);
            if (project != null)
            {
                // Perform soft delete logic
                project.IsDeleted = true;
                project.DeletedAt = DateTime.UtcNow;
                project.DeletedBy = deletedBy;
                _context.Projects.Update(project);
            }
        }

        public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            _context.Projects.Update(project);
            await Task.CompletedTask;
        }
    }
}
