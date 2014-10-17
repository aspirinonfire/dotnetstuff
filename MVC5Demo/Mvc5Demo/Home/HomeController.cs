using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mvc5Demo.Home
{
  public class HomeController : Controller
  {
    private IHomeService homeSvc;

    public HomeController(IHomeService homeSvc)
    {
      this.homeSvc = homeSvc; 
    }

    // GET: all
    public ActionResult All()
    {
      var viewModel = this.homeSvc.getAll();

      return View(viewModel);
    }

    // GET: by name
    public ActionResult Details(string id)
    {
      var viewModels = this.homeSvc.get(id);

      return View(viewModels);
    }
  }
}