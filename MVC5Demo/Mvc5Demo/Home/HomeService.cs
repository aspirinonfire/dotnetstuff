using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mvc5Demo.Home
{
  /// <summary>
  /// Home Service class. Implements business logic. Each method defines a use case
  /// </summary>
  public class HomeService : IHomeService
  {
    private IHomeComponent homeCmp;

    public HomeService(IHomeComponent homeCmp)
    {
      this.homeCmp = homeCmp;
    }

    /// <summary>
    /// Retrieve a single home model and convert it to ViewModel
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public HomeViewModel get(string name)
    {
      HomeViewModel viewModel = null;

      // basic validation
      if (String.IsNullOrWhiteSpace(name))
      {
        return viewModel;
      }

      HomeModel model = this.homeCmp.get(name);

      if (model != null)
      {
        viewModel = new HomeViewModel();
        viewModel.name = model.name;
        viewModel.email = model.email;
      }

      return viewModel;
    }

    /// <summary>
    /// Retrieve a collection of home models and convert them to ViewModels
    /// </summary>
    /// <returns></returns>
    public IEnumerable<HomeViewModel> getAll()
    {
      IEnumerable<HomeViewModel> viewModels = null;
      var models = this.homeCmp.getAll();

      if (models != null && models.Count() > 0)
      {
        viewModels = models.Select(m => new HomeViewModel { name = m.name, email = m.email }).ToList();
      }

      return viewModels;
    }
  }
}