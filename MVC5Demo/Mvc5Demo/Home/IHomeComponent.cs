using System;
using System.Collections.Generic;
namespace Mvc5Demo.Home
{
  public interface IHomeComponent
  {
    HomeModel get(string name);
    IEnumerable<HomeModel> getAll();
  }
}
