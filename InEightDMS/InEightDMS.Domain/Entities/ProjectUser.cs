using System;

namespace InEightDMS.Domain.Entities
{
    /// <summary>
    /// Many-to-many relationship between Projects and Users
    /// </summary>
    public class ProjectUser
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public DateTime AssignedAt { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; }
        public virtual ApplicationUser User { get; set; }

        public ProjectUser()
        {
            AssignedAt = DateTime.UtcNow;
        }
    }
}
