using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using InEightDMS.Data;
using InEightDMS.Data.Repositories;
using InEightDMS.Domain.Entities;
using InEightDMS.Web.Models;

namespace InEightDMS.Web.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly DMSDbContext _context = new DMSDbContext();
        private readonly ProjectRepository _projectRepo;

        public ProjectsController()
        {
            _projectRepo = new ProjectRepository(_context);
        }

        // GET: Projects
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId<int>();
            var projects = await _projectRepo.GetByUserAsync(userId);
            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // GET: Projects/Create
        public async Task<ActionResult> Create()
        {
            var userId = User.Identity.GetUserId<int>();
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);

            // Only Admin can create projects
            if (user.Role != UserRole.Admin)
            {
                return new HttpStatusCodeResult(403, "Only Admin can create projects");
            }

            var managers = await _context.Users
                .Where(u => u.Role == UserRole.ProjectManager && u.IsActive)
                .ToListAsync();
            ViewBag.Managers = new SelectList(managers, "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProjectCreateViewModel model)
        {
            var userId = User.Identity.GetUserId<int>();
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);

            if (user.Role != UserRole.Admin)
            {
                return new HttpStatusCodeResult(403, "Only Admin can create projects");
            }

            if (!ModelState.IsValid)
            {
                var managers = await _context.Users
                    .Where(u => u.Role == UserRole.ProjectManager && u.IsActive)
                    .ToListAsync();
                ViewBag.Managers = new SelectList(managers, "Id", "Name");
                return View(model);
            }

            var project = new Project
            {
                Name = model.Name,
                Description = model.Description,
                ManagerId = model.ManagerId
            };

            await _projectRepo.CreateAsync(project);
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/AddUser/5
        public async Task<ActionResult> AddUser(int id)
        {
            var userId = User.Identity.GetUserId<int>();
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            var project = await _context.Projects.SingleOrDefaultAsync(p => p.Id == id);

            // Only Project Manager of this project can add users
            if (project == null || (user.Role != UserRole.Admin && project.ManagerId != userId))
            {
                return new HttpStatusCodeResult(403);
            }

            var availableUsers = await _context.Users
                .Where(u => u.Role == UserRole.ProjectUser && u.IsActive)
                .ToListAsync();
            ViewBag.Users = new SelectList(availableUsers, "Id", "Name");
            ViewBag.ProjectId = id;
            ViewBag.ProjectName = project.Name;
            return View();
        }

        // POST: Projects/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser(AddUserToProjectViewModel model)
        {
            var userId = User.Identity.GetUserId<int>();
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            var project = await _context.Projects.SingleOrDefaultAsync(p => p.Id == model.ProjectId);

            if (project == null || (user.Role != UserRole.Admin && project.ManagerId != userId))
            {
                return new HttpStatusCodeResult(403);
            }

            await _projectRepo.AddUserAsync(model.ProjectId, model.UserId);
            return RedirectToAction(nameof(Details), new { id = model.ProjectId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
