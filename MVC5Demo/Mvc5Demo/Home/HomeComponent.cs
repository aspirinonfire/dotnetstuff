using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mvc5Demo.Home
{
  /// <summary>
  /// Home Component class. Provides access to HomeDao with validations
  /// </summary>
  public class HomeComponent : IHomeComponent
  {
    private IHomeDao homeDao;

    public HomeComponent(IHomeDao homeDao)
    {
      this.homeDao = homeDao;
    }

    /// <summary>
    /// Get single home model. Throws an ArgumentException if name param is null or empty
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public HomeModel get(string name)
    {
      if (String.IsNullOrWhiteSpace(name))
      {
        throw new ArgumentException("Missing Parameter", "name");
      }

      return this.homeDao.get(name);
    }

    /// <summary>
    /// Get a list of all home models.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<HomeModel> getAll()
    {
      return this.homeDao.getAll();
    }
  }
}