namespace SqlSample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class usercarts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Carts",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        total = c.Double(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        email = c.String(nullable: false, maxLength: 100),
                        name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.email, unique: true, name: "ix_email");
            
            CreateTable(
                "dbo.UserCarts",
                c => new
                    {
                        User_id = c.Int(nullable: false),
                        Cart_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_id, t.Cart_id })
                .ForeignKey("dbo.Users", t => t.User_id, cascadeDelete: true)
                .ForeignKey("dbo.Carts", t => t.Cart_id, cascadeDelete: true)
                .Index(t => t.User_id)
                .Index(t => t.Cart_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserCarts", "Cart_id", "dbo.Carts");
            DropForeignKey("dbo.UserCarts", "User_id", "dbo.Users");
            DropIndex("dbo.UserCarts", new[] { "Cart_id" });
            DropIndex("dbo.UserCarts", new[] { "User_id" });
            DropIndex("dbo.Users", "ix_email");
            DropTable("dbo.UserCarts");
            DropTable("dbo.Users");
            DropTable("dbo.Carts");
        }
    }
}
