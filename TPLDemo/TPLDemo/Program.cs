using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

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
      executeFilteredIntAggregator().Wait();

      Console.WriteLine("=== Done ===");
      Console.ReadLine();
    }


    /// <summary>
    /// Execute demo Dataflow pipeline that aggregates a collection
    /// </summary>
    /// <returns></returns>
    static async Task executeFilteredIntAggregator()
    {
      Stopwatch stopWatch = new Stopwatch();

      // pipeline settings
      int maxItemsInQueue = Environment.ProcessorCount;
      int degreeOfParallelism = Environment.ProcessorCount;

      // filters
      Predicate<int> evenItemFilter = (item) => (item % 2) == 0;
      Predicate<int> oddItemFilter = (item) => (item % 2) != 0;
      Predicate<int> positiveItemFilter = (item) => item >= 0;
      Predicate<int> negativeItemFilter = (item) => item < 0;

      // final consumer
      int sum = 0;
      Action<int> finalConsumer =
        (item) =>
        {
          // TODO adjust this value depending on your computer resources
          for (long i = 0; i < 1000000000; ++i)
          {
            // noop
          }

          /**
           * Sum up filtered item using atomic operation that is thread-safe
           */
          Interlocked.Add(ref sum, item);
        };

      Predicate<int> itemFilter = evenItemFilter;
      LongTaskExecutor<int> longTaskExecutor = new LongTaskExecutor<int>(maxItemsInQueue, degreeOfParallelism, itemFilter, finalConsumer);
      //IEnumerable<int> items = Enumerable.Range(1, 25);
      IEnumerable<int> items = randomIntGenerator(100, 25);

      stopWatch.Start();
      await longTaskExecutor.executePipeline(items);
      stopWatch.Stop();
      Console.WriteLine("Sum of all numbers that pass filter is {0}", sum);

      Console.WriteLine("Execution time: {0}ms", stopWatch.ElapsedMilliseconds);
    }


    /// <summary>
    /// Generate random number with random sign
    /// </summary>
    /// <param name="max">Max value of the random number</param>
    /// <param name="total">Total number of numbers to produce</param>
    /// <returns></returns>
    static IEnumerable<int> randomIntGenerator(int max, int total)
    {
      Random rnd = new Random();

      while (total > 0)
      {
        --total;
        int sign = rnd.Next(2) == 0 ? 1: -1;

        // yield computed random number
        yield return sign * rnd.Next(max);
      }
    }
  }
}
