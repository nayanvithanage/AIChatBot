using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using InEightDMS.Domain.Entities;

namespace InEightDMS.Data
{
    /// <summary>
    /// Entity Framework 6 DbContext with ASP.NET Identity integration
    /// </summary>
    public class DMSDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int,
        ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public DMSDbContext() : base("DMSConnection")
        {
            Configuration.LazyLoadingEnabled = true;
            Configuration.ProxyCreationEnabled = true;
        }

        // DbSets for DMS entities
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentLink> DocumentLinks { get; set; }
        public DbSet<DocumentRevision> DocumentRevisions { get; set; }
        public DbSet<LinkedItem> LinkedItems { get; set; }
        public DbSet<DocumentAction> DocumentActions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ProjectUser composite key
            modelBuilder.Entity<ProjectUser>()
                .HasKey(pu => new { pu.ProjectId, pu.UserId });

            // Project -> Manager relationship
            modelBuilder.Entity<Project>()
                .HasRequired(p => p.Manager)
                .WithMany(u => u.ManagedProjects)
                .HasForeignKey(p => p.ManagerId)
                .WillCascadeOnDelete(false);

            // Document -> Project relationship
            modelBuilder.Entity<Document>()
                .HasRequired(d => d.Project)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.ProjectId)
                .WillCascadeOnDelete(false);

            // Document -> UploadedBy relationship
            modelBuilder.Entity<Document>()
                .HasRequired(d => d.UploadedBy)
                .WithMany(u => u.UploadedDocuments)
                .HasForeignKey(d => d.UploadedById)
                .WillCascadeOnDelete(false);

            // Document -> UpdatedBy (optional)
            modelBuilder.Entity<Document>()
                .HasOptional(d => d.UpdatedBy)
                .WithMany()
                .HasForeignKey(d => d.UpdatedById)
                .WillCascadeOnDelete(false);

            // Document -> Manager (optional)
            modelBuilder.Entity<Document>()
                .HasOptional(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerId)
                .WillCascadeOnDelete(false);

            // Document -> AssignedUser (optional)
            modelBuilder.Entity<Document>()
                .HasOptional(d => d.AssignedUser)
                .WithMany()
                .HasForeignKey(d => d.AssignedUserId)
                .WillCascadeOnDelete(false);

            // Document -> ApprovedBy (optional)
            modelBuilder.Entity<Document>()
                .HasOptional(d => d.ApprovedBy)
                .WithMany()
                .HasForeignKey(d => d.ApprovedById)
                .WillCascadeOnDelete(false);

            // DocumentLink -> SourceDocument
            modelBuilder.Entity<DocumentLink>()
                .HasRequired(dl => dl.SourceDocument)
                .WithMany(d => d.LinkedDocuments)
                .HasForeignKey(dl => dl.SourceDocumentId)
                .WillCascadeOnDelete(false);

            // DocumentLink -> TargetDocument
            modelBuilder.Entity<DocumentLink>()
                .HasRequired(dl => dl.TargetDocument)
                .WithMany()
                .HasForeignKey(dl => dl.TargetDocumentId)
                .WillCascadeOnDelete(false);

            // DocumentRevision -> Document
            modelBuilder.Entity<DocumentRevision>()
                .HasRequired(dr => dr.Document)
                .WithMany(d => d.Revisions)
                .HasForeignKey(dr => dr.DocumentId)
                .WillCascadeOnDelete(false);

            // LinkedItem -> Document
            modelBuilder.Entity<LinkedItem>()
                .HasRequired(li => li.Document)
                .WithMany(d => d.LinkedItems)
                .HasForeignKey(li => li.DocumentId)
                .WillCascadeOnDelete(false);

            // DocumentAction -> Document
            modelBuilder.Entity<DocumentAction>()
                .HasRequired(da => da.Document)
                .WithMany(d => d.Actions)
                .HasForeignKey(da => da.DocumentId)
                .WillCascadeOnDelete(false);

            // Configure string lengths
            modelBuilder.Entity<Document>()
                .Property(d => d.Name).HasMaxLength(500).IsRequired();

            modelBuilder.Entity<Project>()
                .Property(p => p.Name).HasMaxLength(200).IsRequired();
        }

        /// <summary>
        /// Factory method for OWIN authentication
        /// </summary>
        public static DMSDbContext Create()
        {
            return new DMSDbContext();
        }
    }
}
