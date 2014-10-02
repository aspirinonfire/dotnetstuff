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

      #region seed users and carts
      List<Cart> alexCarts = new List<Cart>();
      alexCarts.Add(new Cart { total = 10.0 });
      alexCarts.Add(new Cart { total = null });

      List<Cart> carolCarts = new List<Cart>();
      carolCarts.Add(new Cart { total = 100.0 });

      context.users.AddOrUpdate(
        u => u.email,
        new User { name = "alex", email = "alex@noemail.com", carts = alexCarts },
        new User { name = "carol", email = "carol@noemail.com", carts = carolCarts }
      );
      #endregion

      #region seed categories
      // hierarchy check before seeding
      bool exists =
        context.categories
        .Any(c => c.name.Equals("grand parent category", StringComparison.OrdinalIgnoreCase));

      if (!exists)
      {
        context.categories.Add(
          new Category()
          {
            name = "child category",
            parentCategory = new Category()
            {
              name = "parent category",
              parentCategory = new Category() { name = "grand parent category" }
            }
          }
        );
      }
      #endregion
    }
  }
}
