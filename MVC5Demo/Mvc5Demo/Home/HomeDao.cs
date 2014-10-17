using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mvc5Demo.Home
{
  /// <summary>
  /// Home data access layer.
  /// </summary>
  public class HomeDao : IHomeDao
  {
    // mock models
    private List<HomeModel> models;

    public HomeDao()
    {
      this.models = new List<HomeModel>()
      {
        new HomeModel { email="a@a.a", name="alex", someInternalInfo="super chill dude"},
        new HomeModel {email="k@a.a", name="karolina", someInternalInfo="very pretty" }
      };
    }

    public HomeModel get(string name)
    {
      return models.FirstOrDefault(m => String.Equals(m.name, name, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<HomeModel> getAll()
    {
      return models;
    }
  }
}