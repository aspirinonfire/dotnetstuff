using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvc5Demo.Home
{
  public interface IHomeDao
  {
    HomeModel get(string name);
    IEnumerable<HomeModel> getAll();
  }
}
