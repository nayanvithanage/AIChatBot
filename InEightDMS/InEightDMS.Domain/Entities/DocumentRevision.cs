using System;

namespace InEightDMS.Domain.Entities
{
    /// <summary>
    /// Document revision history
    /// </summary>
    public class DocumentRevision
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int RevisionNumber { get; set; }
        public DateTime RevisionDate { get; set; }
        public string RevisionComment { get; set; }
        public RevisionStatus Status { get; set; }
        public int RevisedById { get; set; }
        public string FilePath { get; set; }

        public virtual Document Document { get; set; }
        public virtual ApplicationUser RevisedBy { get; set; }

        public DocumentRevision()
        {
            RevisionDate = DateTime.UtcNow;
        }
    }
}
