using System;
using System.Collections.Generic;

namespace InEightDMS.Domain.Entities
{
    /// <summary>
    /// Project entity representing a construction project
    /// </summary>
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManagerId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser Manager { get; set; }
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }
        public virtual ICollection<Document> Documents { get; set; }

        public Project()
        {
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            ProjectUsers = new HashSet<ProjectUser>();
            Documents = new HashSet<Document>();
        }
    }
}
