using RVDMS.Application.Queries.Projects;
using RVDMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVDMS.Application.Interfaces
{
    public interface IProjectRepository
    {
        Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Project>> GetFilteredAsync(ProjectFilter filter, CancellationToken cancellationToken = default);
        Task<int> CountAsync(ProjectFilter filter, CancellationToken cancellationToken = default);
        Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
        Task<Project?> GetProjectByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        // Create
        Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default);

       

        // Delete (Soft delete)
        Task SoftDeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);

        // Save changes
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
