namespace InEightDMS.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsActiveToDocument : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Documents", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Documents", "IsActive");
        }
    }
}
