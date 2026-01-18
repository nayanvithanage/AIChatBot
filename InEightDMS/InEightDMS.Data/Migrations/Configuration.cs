namespace InEightDMS.Data.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<InEightDMS.Data.DMSDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(InEightDMS.Data.DMSDbContext context)
        {
            // 1. Seed Roles/Users if they don't exist
            var passwordHasher = new Microsoft.AspNet.Identity.PasswordHasher();
            var passwordHash = passwordHasher.HashPassword("P@ssword123");

            // Seed Admin
            context.Users.AddOrUpdate(u => u.UserName,
                new InEightDMS.Domain.Entities.ApplicationUser
                {
                    UserName = "admin@ineight.com",
                    Email = "admin@ineight.com",
                    Name = "System Admin",
                    Role = InEightDMS.Domain.Entities.UserRole.Admin,
                    IsActive = true,
                    PasswordHash = passwordHash,
                    SecurityStamp = Guid.NewGuid().ToString()
                });

            // Seed Project Manager
            context.Users.AddOrUpdate(u => u.UserName,
                new InEightDMS.Domain.Entities.ApplicationUser
                {
                    UserName = "pm@ineight.com",
                    Email = "pm@ineight.com",
                    Name = "John Manager",
                    Role = InEightDMS.Domain.Entities.UserRole.ProjectManager,
                    IsActive = true,
                    PasswordHash = passwordHash,
                    SecurityStamp = Guid.NewGuid().ToString()
                });

            // Seed Project User
            context.Users.AddOrUpdate(u => u.UserName,
                new InEightDMS.Domain.Entities.ApplicationUser
                {
                    UserName = "user@ineight.com",
                    Email = "user@ineight.com",
                    Name = "Jane Engineer",
                    Role = InEightDMS.Domain.Entities.UserRole.ProjectUser,
                    IsActive = true,
                    PasswordHash = passwordHash,
                    SecurityStamp = Guid.NewGuid().ToString()
                });

            context.SaveChanges();

            // 2. Fetch seeded users for IDs
            var admin = context.Users.First(u => u.UserName == "admin@ineight.com");
            var pm = context.Users.First(u => u.UserName == "pm@ineight.com");
            var user = context.Users.First(u => u.UserName == "user@ineight.com");

            // 3. Seed Projects
            context.Projects.AddOrUpdate(p => p.Name,
                new InEightDMS.Domain.Entities.Project
                {
                    Name = "Alpha Station Construction",
                    Description = "Primary construction project for the new Alpha research station.",
                    ManagerId = pm.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new InEightDMS.Domain.Entities.Project
                {
                    Name = "Beta Pipeline",
                    Description = "Industrial pipeline development for the southern sector.",
                    ManagerId = pm.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });

            context.SaveChanges();

            var projectAlpha = context.Projects.First(p => p.Name == "Alpha Station Construction");

            // 4. Seed Project Users
            if (!context.ProjectUsers.Any(pu => pu.ProjectId == projectAlpha.Id && pu.UserId == user.Id))
            {
                context.ProjectUsers.Add(new InEightDMS.Domain.Entities.ProjectUser
                {
                    ProjectId = projectAlpha.Id,
                    UserId = user.Id
                });
            }

            // 5. Seed Documents
            context.Documents.AddOrUpdate(d => d.Name,
                new InEightDMS.Domain.Entities.Document
                {
                    ProjectId = projectAlpha.Id,
                    Name = "Structural Plan - Level 1",
                    Description = "Detailed structural blueprints for the first level floor plan.",
                    Type = "PDF",
                    Category = "Engineering",
                    Tags = "structural,blueprints,level1",
                    Status = InEightDMS.Domain.Entities.DocumentStatus.IFC,
                    Version = 1,
                    RevisionNumber = 0,
                    UploadedById = user.Id,
                    UploadedAt = DateTime.UtcNow.AddDays(-5),
                    IsActive = true
                },
                new InEightDMS.Domain.Entities.Document
                {
                    ProjectId = projectAlpha.Id,
                    Name = "Safety Protocol Manual",
                    Description = "Standard safety protocols for on-site operations.",
                    Type = "DOCX",
                    Category = "Safety",
                    Tags = "safety,manual,operations",
                    Status = InEightDMS.Domain.Entities.DocumentStatus.Approved,
                    Version = 2,
                    RevisionNumber = 1,
                    UploadedById = pm.Id,
                    UploadedAt = DateTime.UtcNow.AddDays(-10),
                    IsActive = true
                });

            context.SaveChanges();

            var docBlueprints = context.Documents.First(d => d.Name == "Structural Plan - Level 1");

            // 6. Seed Linked Items
            context.LinkedItems.AddOrUpdate(l => l.ItemNumber,
                new InEightDMS.Domain.Entities.LinkedItem
                {
                    DocumentId = docBlueprints.Id,
                    ItemType = InEightDMS.Domain.Entities.LinkedItemType.RFI,
                    ItemNumber = "RFI-001",
                    Title = "Clarification on beam placement",
                    Status = InEightDMS.Domain.Entities.ItemStatus.Open,
                    CreatedById = user.Id,
                    CreatedAt = DateTime.UtcNow
                });
        }
    }
}
