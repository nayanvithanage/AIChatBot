using System;

namespace InEightDMS.Domain.Entities
{
    /// <summary>
    /// Document action tracking (audit log)
    /// </summary>
    public class DocumentAction
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public ActionType Type { get; set; }
        public int ActionById { get; set; }
        public DateTime ActionAt { get; set; }
        public string ToolUsed { get; set; } // Bluebeam, etc.
        public string Details { get; set; } // JSON

        public virtual Document Document { get; set; }
        public virtual ApplicationUser ActionBy { get; set; }

        public DocumentAction()
        {
            ActionAt = DateTime.UtcNow;
        }
    }
}
