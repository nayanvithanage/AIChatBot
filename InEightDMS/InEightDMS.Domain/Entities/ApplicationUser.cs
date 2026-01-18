using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace InEightDMS.Domain.Entities
{
    /// <summary>
    /// Application user with ASP.NET Identity integration
    /// </summary>
    public class ApplicationUser : IdentityUser<int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public string Name { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<ProjectUser> ProjectAssignments { get; set; }
        public virtual ICollection<Project> ManagedProjects { get; set; }
        public virtual ICollection<Document> UploadedDocuments { get; set; }

        public ApplicationUser()
        {
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            ProjectAssignments = new HashSet<ProjectUser>();
            ManagedProjects = new HashSet<Project>();
            UploadedDocuments = new HashSet<Document>();
        }

        /// <summary>
        /// Generates user identity for OWIN cookie authentication
        /// </summary>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            userIdentity.AddClaim(new Claim("UserRole", Role.ToString()));
            userIdentity.AddClaim(new Claim("Name", Name ?? ""));
            return userIdentity;
        }
    }

    /// <summary>
    /// ASP.NET Identity user login
    /// </summary>
    public class ApplicationUserLogin : IdentityUserLogin<int> { }

    /// <summary>
    /// ASP.NET Identity user role mapping
    /// </summary>
    public class ApplicationUserRole : IdentityUserRole<int> { }

    /// <summary>
    /// ASP.NET Identity user claim
    /// </summary>
    public class ApplicationUserClaim : IdentityUserClaim<int> { }

    /// <summary>
    /// ASP.NET Identity role
    /// </summary>
    public class ApplicationRole : IdentityRole<int, ApplicationUserRole>
    {
        public ApplicationRole() { }
        public ApplicationRole(string name) : this() { Name = name; }
    }
}
