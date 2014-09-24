namespace SqlSample.Migrations
{
  using SqlSample.db;
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Data.Entity.Migrations;
  using System.Linq;

  /// <summary>
  /// Automatic database migration configuration for Entity Framework
  /// </summary>
  internal sealed class Configuration : DbMigrationsConfiguration<SqlSample.db.MyDBContext>
  {
    public Configuration()
    {
      AutomaticMigrationsEnabled = true;
      // DANGER! Dataloss is possible because of this flag
      AutomaticMigrationDataLossAllowed = true;
    }

    protected override void Seed(MyDBContext context)
    {
      //  This method will be called after migrating to the latest version.

      //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
      //  to avoid creating duplicate seed data. E.g.
      //
      //    context.People.AddOrUpdate(
      //      p => p.FullName,
      //      new Person { FullName = "Andrew Peters" },
      //      new Person { FullName = "Brice Lambson" },
      //      new Person { FullName = "Rowan Miller" }
      //    );
      //

      List<Cart> alexCarts = new List<Cart>();
      alexCarts.Add(new Cart { total = 10.0 });
      alexCarts.Add(new Cart { total = null });

      List<Cart> carolCarts = new List<Cart>();
      carolCarts.Add(new Cart { total = 100.0 });

      // test data
      context.users.AddOrUpdate(
        u => u.email,
        new User { name = "alex", email = "alex@noemail.com", carts = alexCarts },
        new User { name = "carol", email = "carol@noemail.com", carts = carolCarts }
      );
    }
  }
}
