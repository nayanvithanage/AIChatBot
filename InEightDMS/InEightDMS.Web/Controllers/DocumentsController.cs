using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using InEightDMS.Data;
using InEightDMS.Data.Repositories;
using InEightDMS.Domain.Entities;
using InEightDMS.Web.Models;

namespace InEightDMS.Web.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly DMSDbContext _context = new DMSDbContext();
        private readonly DocumentRepository _docRepo;
        private readonly string _uploadPath = "~/App_Data/Uploads";

        public DocumentsController()
        {
            _docRepo = new DocumentRepository(_context);
        }

        // GET: Documents?projectId=5
        public async Task<ActionResult> Index(int projectId)
        {
            var userId = User.Identity.GetUserId<int>();
            var documents = await _docRepo.GetAccessibleDocumentsAsync(userId, projectId);

            if (!documents.Any())
            {
                // Check if user has access to this project
                var hasAccess = await HasProjectAccessAsync(userId, projectId);
                if (!hasAccess)
                {
                    return new HttpStatusCodeResult(403, "You don't have access to this project");
                }
            }

            var project = await _context.Projects.SingleOrDefaultAsync(p => p.Id == projectId);
            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = project?.Name ?? "Unknown Project";
            return View(documents);
        }

        // GET: Documents/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var document = await _docRepo.GetByIdAsync(id);
            if (document == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId<int>();
            var hasAccess = await HasProjectAccessAsync(userId, document.ProjectId);
            if (!hasAccess)
            {
                return new HttpStatusCodeResult(403);
            }

            return View(document);
        }

        // GET: Documents/Create?projectId=5
        public async Task<ActionResult> Create(int projectId)
        {
            var userId = User.Identity.GetUserId<int>();
            var hasAccess = await HasProjectAccessAsync(userId, projectId);
            if (!hasAccess)
            {
                return new HttpStatusCodeResult(403);
            }

            var project = await _context.Projects.SingleOrDefaultAsync(p => p.Id == projectId);
            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = project?.Name ?? "Unknown Project";
            ViewBag.DocumentTypes = new SelectList(new[] { "IFC", "DWG", "PDF", "DOCX", "XLSX", "Other" });
            return View();
        }

        // POST: Documents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DocumentCreateViewModel model, HttpPostedFileBase file)
        {
            var userId = User.Identity.GetUserId<int>();
            var hasAccess = await HasProjectAccessAsync(userId, model.ProjectId);
            if (!hasAccess)
            {
                return new HttpStatusCodeResult(403);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ProjectId = model.ProjectId;
                ViewBag.DocumentTypes = new SelectList(new[] { "IFC", "DWG", "PDF", "DOCX", "XLSX", "Other" });
                return View(model);
            }

            // Handle file upload
            string filePath = null;
            if (file != null && file.ContentLength > 0)
            {
                var uploadDir = Server.MapPath(_uploadPath);
                Directory.CreateDirectory(uploadDir);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var fullPath = Path.Combine(uploadDir, fileName);
                file.SaveAs(fullPath);
                filePath = fileName;
            }

            var document = new Document
            {
                ProjectId = model.ProjectId,
                Name = model.Name,
                Description = model.Description,
                Type = model.Type,
                Category = model.Category,
                Tags = model.Tags,
                TransmittalNumber = model.TransmittalNumber,
                Status = DocumentStatus.Draft,
                UploadedById = userId,
                UploadedAt = DateTime.UtcNow,
                FilePath = filePath
            };

            await _docRepo.CreateAsync(document);

            // Log action
            await _docRepo.AddActionAsync(new DocumentAction
            {
                DocumentId = document.Id,
                Type = ActionType.Created,
                ActionById = userId,
                Details = $"{{\"name\":\"{document.Name}\",\"type\":\"{document.Type}\"}}"
            });

            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        // GET: Documents/AddLinkedItem/5
        public async Task<ActionResult> AddLinkedItem(int id)
        {
            var document = await _context.Documents.SingleOrDefaultAsync(d => d.Id == id);
            if (document == null)
            {
                return HttpNotFound();
            }

            ViewBag.DocumentId = id;
            ViewBag.DocumentName = document.Name;
            ViewBag.ItemTypes = new SelectList(Enum.GetValues(typeof(LinkedItemType)));
            return View();
        }

        // POST: Documents/AddLinkedItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddLinkedItem(LinkedItemViewModel model)
        {
            var userId = User.Identity.GetUserId<int>();

            if (!ModelState.IsValid)
            {
                ViewBag.DocumentId = model.DocumentId;
                ViewBag.ItemTypes = new SelectList(Enum.GetValues(typeof(LinkedItemType)));
                return View(model);
            }

            var linkedItem = new LinkedItem
            {
                DocumentId = model.DocumentId,
                ItemType = model.ItemType,
                Status = ItemStatus.Open,
                ItemNumber = model.ItemNumber,
                Title = model.Title,
                CreatedById = userId
            };

            await _docRepo.AddLinkedItemAsync(linkedItem);
            return RedirectToAction(nameof(Details), new { id = model.DocumentId });
        }

        private async Task<bool> HasProjectAccessAsync(int userId, int projectId)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            if (user.Role == UserRole.Admin) return true;

            return await _context.ProjectUsers
                .AnyAsync(pu => pu.UserId == userId && pu.ProjectId == projectId)
                || await _context.Projects
                .AnyAsync(p => p.Id == projectId && p.ManagerId == userId);
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
