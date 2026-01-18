using System;

namespace InEightDMS.Domain.Entities
{
    /// <summary>
    /// Document-to-document links
    /// </summary>
    public class DocumentLink
    {
        public int Id { get; set; }
        public int SourceDocumentId { get; set; }
        public int TargetDocumentId { get; set; }
        public LinkType Type { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Document SourceDocument { get; set; }
        public virtual Document TargetDocument { get; set; }

        public DocumentLink()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
