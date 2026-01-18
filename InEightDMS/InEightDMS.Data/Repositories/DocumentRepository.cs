using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using InEightDMS.Domain.Entities;

namespace InEightDMS.Data.Repositories
{
    /// <summary>
    /// Repository for Document operations
    /// </summary>
    public class DocumentRepository
    {
        private readonly DMSDbContext _context;

        public DocumentRepository(DMSDbContext context)
        {
            _context = context;
        }

        public async Task<Document> GetByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.UploadedBy)
                .Include(d => d.Project)
                .Include(d => d.LinkedItems)
                .Include(d => d.Actions)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Document>> GetByProjectAsync(int projectId)
        {
            return await _context.Documents
                .Where(d => d.ProjectId == projectId)
                .Include(d => d.UploadedBy)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetAccessibleDocumentsAsync(int userId, int projectId)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null) return new List<Document>();

            // Admin can see all
            if (user.Role == UserRole.Admin)
            {
                return await GetByProjectAsync(projectId);
            }

            // Check project access
            var hasAccess = await _context.ProjectUsers
                .AnyAsync(pu => pu.UserId == userId && pu.ProjectId == projectId)
                || await _context.Projects
                .AnyAsync(p => p.Id == projectId && p.ManagerId == userId);

            if (!hasAccess) return new List<Document>();

            return await GetByProjectAsync(projectId);
        }

        public async Task<Document> CreateAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task UpdateAsync(Document document)
        {
            _context.Entry(document).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var document = await _context.Documents.SingleOrDefaultAsync(d => d.Id == id);
            if (document != null)
            {
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddLinkedItemAsync(LinkedItem item)
        {
            _context.LinkedItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task AddActionAsync(DocumentAction action)
        {
            _context.DocumentActions.Add(action);
            await _context.SaveChangesAsync();
        }
    }
}
