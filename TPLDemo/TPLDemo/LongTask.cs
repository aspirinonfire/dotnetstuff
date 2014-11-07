using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TPLDemo
{
  public class LongTask
  {
    /// <summary>
    /// process item with a long wait
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static async Task<int> runLong(int item)
    {
      await Task.Factory.StartNew(
        () =>
        {
          for (long i = 0; i < 1000000000; ++i)
          {
            // noop
          }
        });
      return item;
    }
  }
}
