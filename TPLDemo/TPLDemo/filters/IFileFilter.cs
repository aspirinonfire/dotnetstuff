using System;
namespace TPLDemo.filters
{
  public interface IFileFilter
  {
    bool execute(string filePath);
  }
}
