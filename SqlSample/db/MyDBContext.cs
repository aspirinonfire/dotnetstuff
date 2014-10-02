using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSample.db
{
  /// <summary>
  /// Entity Framework custom database context. Code-first approach
  /// </summary>
  /// <remarks>
  /// enable-migrations –EnableAutomaticMigration:$true
  /// </remarks>
  public class MyDBContext : DbContext
  {
    public MyDBContext()
      : base("MyDBConnectionString")
    {
      Database.SetInitializer
        (new MigrateDatabaseToLatestVersion<MyDBContext, Migrations.Configuration>("MyDBConnectionString"));
    }

    public DbSet<User> users { get; set; }
    public DbSet<Cart> carts { get; set; }
    public DbSet<Category> categories { get; set; }
    public DbSet<Visitor> visitors { get; set; }
  }
}
