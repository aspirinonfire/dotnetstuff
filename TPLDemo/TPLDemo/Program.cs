using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using TPLDemo.filters;
using System.Linq;

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
      Stopwatch stopWatch = new Stopwatch();

      //string targetPath = @"C:\Users\alexc.alexc-pc\Desktop\goalcoach_nodejs\goalcoach_nodejs";
      //FileProcessor processor = new FileProcessor();
      //processor.addParallelFileFilter(new ExtensionFilter("js"));
      //string[] paths = processor.traverseDirectory(targetPath);
      //stopWatch.Start();
      //var task = processor.run(paths);
      //task.Wait();
      //stopWatch.Stop();
      //Console.WriteLine("Total line count of filtered files: {0}", task.Result);

      // create long task that executes for 1000ms
      int maxItemsInQueue = 2;
      int degreeOfParallelism = 2;
      int numberOfItemsToProcess = 10;

      LongTaskExecutor executor = new LongTaskExecutor(maxItemsInQueue, degreeOfParallelism);
      var numbers = Enumerable.Range(1, numberOfItemsToProcess).ToArray();

      stopWatch.Start();
      var t = executor.runSlowIntSum(numbers);
      t.Wait();
      stopWatch.Stop();
      Console.WriteLine("Sum of all numbers between {0} and {1} is {2}", 1, numberOfItemsToProcess, t.Result);

      Console.WriteLine("Execution time: {0}ms", stopWatch.ElapsedMilliseconds);

      Console.WriteLine("=== Done ===");
      Console.ReadLine();
    }

    static IEnumerable<int> sequentialNumGenerator(int start, int end)
    {
      for (int i = start; i < end; ++i)
      {
        yield return i;
      }
    }
  }
}
