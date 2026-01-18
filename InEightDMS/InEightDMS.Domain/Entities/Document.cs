using System;
using System.Collections.Generic;

namespace InEightDMS.Domain.Entities
{
    /// <summary>
    /// Document entity with all 26+ metadata fields
    /// </summary>
    public class Document
    {
        // Core Fields
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // IFC, DWG, PDF, etc.
        public string Category { get; set; }
        public string Tags { get; set; } // Comma-separated
        public bool IsActive { get; set; }

        // Upload Information
        public int UploadedById { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadLocation { get; set; }

        // Update Information
        public int? UpdatedById { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Status & Workflow
        public DocumentStatus Status { get; set; }
        public string TransmittalNumber { get; set; }

        // Version Control
        public int Version { get; set; }
        public int RevisionNumber { get; set; }

        // Assignment
        public int? ManagerId { get; set; }
        public int? AssignedUserId { get; set; }

        // Approval Workflow
        public ApprovalStatus ApprovalStatus { get; set; }
        public int? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string ApprovalComment { get; set; }

        // Revision Tracking
        public DateTime? RevisionDate { get; set; }
        public string RevisionComment { get; set; }
        public RevisionStatus? RevisionStatusValue { get; set; }

        // File Storage Path
        public string FilePath { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; }
        public virtual ApplicationUser UploadedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ApplicationUser Manager { get; set; }
        public virtual ApplicationUser AssignedUser { get; set; }
        public virtual ApplicationUser ApprovedBy { get; set; }

        public virtual ICollection<DocumentLink> LinkedDocuments { get; set; }
        public virtual ICollection<DocumentRevision> Revisions { get; set; }
        public virtual ICollection<LinkedItem> LinkedItems { get; set; }
        public virtual ICollection<DocumentAction> Actions { get; set; }

        public Document()
        {
            UploadedAt = DateTime.UtcNow;
            Status = DocumentStatus.Draft;
            ApprovalStatus = ApprovalStatus.Pending;
            IsActive = true;
            Version = 1;
            RevisionNumber = 0;
            LinkedDocuments = new HashSet<DocumentLink>();
            Revisions = new HashSet<DocumentRevision>();
            LinkedItems = new HashSet<LinkedItem>();
            Actions = new HashSet<DocumentAction>();
        }
    }
}
