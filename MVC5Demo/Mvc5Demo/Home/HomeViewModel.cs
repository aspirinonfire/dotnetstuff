using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mvc5Demo.Global;

namespace Mvc5Demo.Home
{
  public class HomeViewModel : ViewModelBase
  {
    public string name { get; set; }
    public string email { get; set; }
  }
}