using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mvc5Demo.Authentication
{
  /// <summary>
  /// Authentication controller
  /// http://www.khalidabuhakmeh.com/asp-net-mvc-5-authentication-breakdown-part-deux
  /// http://blogs.msdn.com/b/webdev/archive/2013/07/03/understanding-owin-forms-authentication-in-mvc-5.aspx
  /// </summary>
  public class AuthenticationController : Controller
  {
    // GET: Authentication
    public ActionResult Login()
    {
      return View();
    }
  }
}