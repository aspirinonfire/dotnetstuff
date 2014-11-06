using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using TPLDemo.filters;
using System.Threading;
using System.IO;

namespace TPLDemo
{
  /// <summary>
  /// Process files
  /// </summary>
  public class FileProcessor
  {
    private readonly List<IFileFilter> parallelFileFilters;

    public FileProcessor()
    {
      this.parallelFileFilters = new List<IFileFilter>();
    }


    /// <summary>
    /// Add file filter that will be executed in parallel with other file filters
    /// </summary>
    /// <param name="fileFilter"></param>
    public void addParallelFileFilter(IFileFilter fileFilter)
    {
      this.parallelFileFilters.Add(fileFilter);
    }


    /// <summary>
    /// Process files
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    public async Task<int> run(params string[] paths)
    {
      int lineSum = 0;
      List<Task> queuedCompletions = new List<Task>();

      #region set up data flow pipeline
      // set up broadcast block which will enqueue file paths
      // throttle number of items available in queue at the time
      var srcOpts = new DataflowBlockOptions { BoundedCapacity = 100 };
      var srcBlk = new BroadcastBlock<string>(path => path);

      // set up parallel filters
      TransformBlock<string, string> processFilter;
      BatchBlock<string> filterResults = new BatchBlock<string>(this.parallelFileFilters.Count, new GroupingDataflowBlockOptions { Greedy = false });
      foreach (IFileFilter filter in this.parallelFileFilters)
      {
        processFilter = new TransformBlock<string, string>(
          (target) =>
          {
            return filter.execute(target) ? target : null;
          });

        srcBlk.LinkTo(processFilter, new DataflowLinkOptions { PropagateCompletion = true });
        processFilter.LinkTo(filterResults, new DataflowLinkOptions { PropagateCompletion = true });
      }
      TransformBlock<IEnumerable<string>, string> filtersResultParser =
        new TransformBlock<IEnumerable<string>,string>(r => r.FirstOrDefault(f => !String.IsNullOrWhiteSpace(f)));
      filterResults.LinkTo(filtersResultParser, new DataflowLinkOptions { PropagateCompletion = true });


      // set up completion action
      ActionBlock<string> saveFiltered = new ActionBlock<string>(
      f =>
      {
        if (!String.IsNullOrWhiteSpace(f) && f.Length < 260)
        {
          int currentLineCount = File.ReadAllLines(f).Length;

          Interlocked.Add(ref lineSum, currentLineCount);
        }
      }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 100, MaxMessagesPerTask = 100 });
      filtersResultParser.LinkTo(saveFiltered, new DataflowLinkOptions { PropagateCompletion = true });
      queuedCompletions.Add(saveFiltered.Completion);
      #endregion

      #region execute dataflow pipeline
      // add items to the pipeline
      foreach (string path in paths)
      {
        await srcBlk.SendAsync(path);
      }
      srcBlk.Complete();
      #endregion

      await Task.WhenAll(queuedCompletions);
      return lineSum;
    }
  }
}
