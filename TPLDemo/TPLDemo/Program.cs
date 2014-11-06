using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using TPLDemo.filters;

namespace TPLDemo
{
  /// <summary>
  /// Dataflow Task Parallel Library Demo
  /// 
  /// http://www.intertech.com/Blog/async-ctp-tpl-dataflow-buffering-and-combining-data/
  /// http://blog.stephencleary.com/2012/11/async-producerconsumer-queue-using.html
  /// </summary>
  class Program
  {

    static void Main(string[] args)
    {
      string targetPath = @"C:\Users\alexc.alexc-pc\Desktop\goalcoach_nodejs\goalcoach_nodejs";
      string[] paths = traverseDirectory(targetPath);

      FileProcessor processor = new FileProcessor();
      processor.addParallelFileFilter(new ExtensionFilter("js"));

      Stopwatch stopWatch = new Stopwatch();

      stopWatch.Start();
      var task = processor.run(paths);
      task.Wait();
      stopWatch.Stop();
      Console.WriteLine("Execution time: {0}ms", stopWatch.ElapsedMilliseconds);
      Console.WriteLine("Total line count of filtered files: {0}", task.Result);

      Console.WriteLine("=== Done ===");
      Console.ReadLine();
    }

    /// <summary>
    /// Recursively traverse target path and retrieve all files
    /// </summary>
    /// <param name="targetPath"></param>
    /// <returns></returns>
    static string[] traverseDirectory(string targetPath)
    {
      if (Directory.Exists(targetPath))
      {
        string[] filePaths = Directory.GetFiles(targetPath, "*", SearchOption.AllDirectories);
        
        return filePaths;
      }
      else
      {
        return new string[0];
      }
    }
  }
}
