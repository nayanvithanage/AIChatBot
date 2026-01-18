using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using InEightDMS.Domain.Entities;

namespace InEightDMS.Data.Repositories
{
    /// <summary>
    /// Repository for Project operations
    /// </summary>
    public class ProjectRepository
    {
        private readonly DMSDbContext _context;

        public ProjectRepository(DMSDbContext context)
        {
            _context = context;
        }

        public async Task<Project> GetByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.ProjectUsers)
                .Include(p => p.Documents)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects
                .Include(p => p.Manager)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetByUserAsync(int userId)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null) return new List<Project>();

            // Admin can see all
            if (user.Role == UserRole.Admin)
            {
                return await GetAllAsync();
            }

            // Get projects where user is manager or assigned
            var managedProjects = await _context.Projects
                .Where(p => p.ManagerId == userId && p.IsActive)
                .Include(p => p.Manager)
                .ToListAsync();

            var assignedProjectIds = await _context.ProjectUsers
                .Where(pu => pu.UserId == userId)
                .Select(pu => pu.ProjectId)
                .ToListAsync();

            var assignedProjects = await _context.Projects
                .Where(p => assignedProjectIds.Contains(p.Id) && p.IsActive)
                .Include(p => p.Manager)
                .ToListAsync();

            return managedProjects.Union(assignedProjects).Distinct().OrderBy(p => p.Name);
        }

        public async Task<Project> CreateAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task UpdateAsync(Project project)
        {
            _context.Entry(project).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task AddUserAsync(int projectId, int userId)
        {
            var exists = await _context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

            if (!exists)
            {
                _context.ProjectUsers.Add(new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = userId
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserAsync(int projectId, int userId)
        {
            var projectUser = await _context.ProjectUsers
                .FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

            if (projectUser != null)
            {
                _context.ProjectUsers.Remove(projectUser);
                await _context.SaveChangesAsync();
            }
        }
    }
}
