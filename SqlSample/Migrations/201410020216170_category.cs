namespace SqlSample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class category : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserCarts", "User_id", "dbo.Users");
            DropForeignKey("dbo.UserCarts", "Cart_id", "dbo.Carts");
            DropIndex("dbo.UserCarts", new[] { "User_id" });
            DropIndex("dbo.UserCarts", new[] { "Cart_id" });
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 50),
                        parentCategoryId = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Categories", t => t.parentCategoryId)
                .Index(t => t.parentCategoryId);
            
            AddColumn("dbo.Carts", "User_id", c => c.Int());
            CreateIndex("dbo.Carts", "User_id");
            AddForeignKey("dbo.Carts", "User_id", "dbo.Users", "id");
            DropTable("dbo.UserCarts");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserCarts",
                c => new
                    {
                        User_id = c.Int(nullable: false),
                        Cart_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_id, t.Cart_id });
            
            DropForeignKey("dbo.Carts", "User_id", "dbo.Users");
            DropForeignKey("dbo.Categories", "parentCategoryId", "dbo.Categories");
            DropIndex("dbo.Categories", new[] { "parentCategoryId" });
            DropIndex("dbo.Carts", new[] { "User_id" });
            DropColumn("dbo.Carts", "User_id");
            DropTable("dbo.Categories");
            CreateIndex("dbo.UserCarts", "Cart_id");
            CreateIndex("dbo.UserCarts", "User_id");
            AddForeignKey("dbo.UserCarts", "Cart_id", "dbo.Carts", "id", cascadeDelete: true);
            AddForeignKey("dbo.UserCarts", "User_id", "dbo.Users", "id", cascadeDelete: true);
        }
    }
}
