using System;
using System.Collections.Generic;
namespace Mvc5Demo.Home
{
  public interface IHomeService
  {
    HomeViewModel get(string name);
    IEnumerable<HomeViewModel> getAll();
  }
}
