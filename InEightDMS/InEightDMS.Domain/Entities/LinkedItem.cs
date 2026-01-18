using System;

namespace InEightDMS.Domain.Entities
{
    /// <summary>
    /// Linked items (RFI, Transmittal, Mail, Form, Task)
    /// </summary>
    public class LinkedItem
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public LinkedItemType ItemType { get; set; }
        public ItemStatus Status { get; set; }
        public string ItemNumber { get; set; }
        public string Title { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Metadata { get; set; } // JSON for additional fields

        public virtual Document Document { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }

        public LinkedItem()
        {
            CreatedAt = DateTime.UtcNow;
            Status = ItemStatus.Open;
        }
    }
}
