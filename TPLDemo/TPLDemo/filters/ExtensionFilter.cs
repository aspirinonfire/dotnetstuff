using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPLDemo.filters
{
  public class ExtensionFilter : IFileFilter
  {
    private readonly string targetExtension;

    public ExtensionFilter(string targetExtension)
    {
      this.targetExtension = "." + targetExtension;
    }

    /// <summary>
    /// Check if supplied file path matches expected extension
    /// </summary>
    /// <param name="targetPath"></param>
    /// <returns></returns>
    public bool execute(string filePath)
    {
      return Path.GetExtension(filePath).Equals(this.targetExtension, StringComparison.OrdinalIgnoreCase);
    }
  }
}
