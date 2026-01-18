namespace InEightDMS.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentActions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentId = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        ActionById = c.Int(nullable: false),
                        ActionAt = c.DateTime(nullable: false),
                        ToolUsed = c.String(),
                        Details = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ActionById, cascadeDelete: true)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .Index(t => t.DocumentId)
                .Index(t => t.ActionById);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Role = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Description = c.String(),
                        ManagerId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ManagerId)
                .Index(t => t.ManagerId);
            
            CreateTable(
                "dbo.Documents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProjectId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 500),
                        Description = c.String(),
                        Type = c.String(),
                        Category = c.String(),
                        Tags = c.String(),
                        UploadedById = c.Int(nullable: false),
                        UploadedAt = c.DateTime(nullable: false),
                        UploadLocation = c.String(),
                        UpdatedById = c.Int(),
                        UpdatedAt = c.DateTime(),
                        Status = c.Int(nullable: false),
                        TransmittalNumber = c.String(),
                        Version = c.Int(nullable: false),
                        RevisionNumber = c.Int(nullable: false),
                        ManagerId = c.Int(),
                        AssignedUserId = c.Int(),
                        ApprovalStatus = c.Int(nullable: false),
                        ApprovedById = c.Int(),
                        ApprovedAt = c.DateTime(),
                        ApprovalComment = c.String(),
                        RevisionDate = c.DateTime(),
                        RevisionComment = c.String(),
                        RevisionStatusValue = c.Int(),
                        FilePath = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApprovedById)
                .ForeignKey("dbo.AspNetUsers", t => t.AssignedUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.ManagerId)
                .ForeignKey("dbo.Projects", t => t.ProjectId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedById)
                .ForeignKey("dbo.AspNetUsers", t => t.UploadedById)
                .Index(t => t.ProjectId)
                .Index(t => t.UploadedById)
                .Index(t => t.UpdatedById)
                .Index(t => t.ManagerId)
                .Index(t => t.AssignedUserId)
                .Index(t => t.ApprovedById);
            
            CreateTable(
                "dbo.DocumentLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SourceDocumentId = c.Int(nullable: false),
                        TargetDocumentId = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.SourceDocumentId)
                .ForeignKey("dbo.Documents", t => t.TargetDocumentId)
                .Index(t => t.SourceDocumentId)
                .Index(t => t.TargetDocumentId);
            
            CreateTable(
                "dbo.LinkedItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentId = c.Int(nullable: false),
                        ItemType = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        ItemNumber = c.String(),
                        Title = c.String(),
                        CreatedById = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        Metadata = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedById, cascadeDelete: true)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .Index(t => t.DocumentId)
                .Index(t => t.CreatedById);
            
            CreateTable(
                "dbo.DocumentRevisions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentId = c.Int(nullable: false),
                        RevisionNumber = c.Int(nullable: false),
                        RevisionDate = c.DateTime(nullable: false),
                        RevisionComment = c.String(),
                        Status = c.Int(nullable: false),
                        RevisedById = c.Int(nullable: false),
                        FilePath = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .ForeignKey("dbo.AspNetUsers", t => t.RevisedById, cascadeDelete: true)
                .Index(t => t.DocumentId)
                .Index(t => t.RevisedById);
            
            CreateTable(
                "dbo.ProjectUsers",
                c => new
                    {
                        ProjectId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        AssignedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.ProjectId, t.UserId })
                .ForeignKey("dbo.Projects", t => t.ProjectId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ProjectId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.DocumentActions", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.DocumentActions", "ActionById", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ProjectUsers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ProjectUsers", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.Projects", "ManagerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Documents", "UploadedById", "dbo.AspNetUsers");
            DropForeignKey("dbo.Documents", "UpdatedById", "dbo.AspNetUsers");
            DropForeignKey("dbo.DocumentRevisions", "RevisedById", "dbo.AspNetUsers");
            DropForeignKey("dbo.DocumentRevisions", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.Documents", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.Documents", "ManagerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.LinkedItems", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.LinkedItems", "CreatedById", "dbo.AspNetUsers");
            DropForeignKey("dbo.DocumentLinks", "TargetDocumentId", "dbo.Documents");
            DropForeignKey("dbo.DocumentLinks", "SourceDocumentId", "dbo.Documents");
            DropForeignKey("dbo.Documents", "AssignedUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Documents", "ApprovedById", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.ProjectUsers", new[] { "UserId" });
            DropIndex("dbo.ProjectUsers", new[] { "ProjectId" });
            DropIndex("dbo.DocumentRevisions", new[] { "RevisedById" });
            DropIndex("dbo.DocumentRevisions", new[] { "DocumentId" });
            DropIndex("dbo.LinkedItems", new[] { "CreatedById" });
            DropIndex("dbo.LinkedItems", new[] { "DocumentId" });
            DropIndex("dbo.DocumentLinks", new[] { "TargetDocumentId" });
            DropIndex("dbo.DocumentLinks", new[] { "SourceDocumentId" });
            DropIndex("dbo.Documents", new[] { "ApprovedById" });
            DropIndex("dbo.Documents", new[] { "AssignedUserId" });
            DropIndex("dbo.Documents", new[] { "ManagerId" });
            DropIndex("dbo.Documents", new[] { "UpdatedById" });
            DropIndex("dbo.Documents", new[] { "UploadedById" });
            DropIndex("dbo.Documents", new[] { "ProjectId" });
            DropIndex("dbo.Projects", new[] { "ManagerId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.DocumentActions", new[] { "ActionById" });
            DropIndex("dbo.DocumentActions", new[] { "DocumentId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.ProjectUsers");
            DropTable("dbo.DocumentRevisions");
            DropTable("dbo.LinkedItems");
            DropTable("dbo.DocumentLinks");
            DropTable("dbo.Documents");
            DropTable("dbo.Projects");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.DocumentActions");
        }
    }
}
