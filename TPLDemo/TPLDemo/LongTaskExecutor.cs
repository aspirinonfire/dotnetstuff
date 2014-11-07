using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using System.Threading;

namespace TPLDemo
{
  public class LongTaskExecutor
  {
    private readonly LongTask longTask;
    private readonly int maxInSrcQ;
    private readonly int degreeOfPrl;
    

    public LongTaskExecutor(int maxInSrcQ, int degreeOfPrl)
    {
      this.maxInSrcQ = maxInSrcQ;
      this.degreeOfPrl = degreeOfPrl;
    }

    public async Task<int> runSlowIntSum(int[] numbers)
    {
      int sum = 0;
      List<Task> queuedCompletions = new List<Task>();

      // pipeline source
      var taskSrcDataBlk = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = maxInSrcQ });
      queuedCompletions.Add(taskSrcDataBlk.Completion);

      // pipeline long task
      var longTaskBlk = new TransformBlock<int, int>(
        (item) =>
        {
          Console.WriteLine("Processing {0}", item);
          return LongTask.runLong(item);
        }, 
        new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = degreeOfPrl});
      queuedCompletions.Add(longTaskBlk.Completion);

      // pipeline completion task - sum all items
      var sumBlk = new ActionBlock<int>(
        (item) =>
        {
          Console.WriteLine("summing {0}", item);
          Interlocked.Add(ref sum, item);
        }, 
        new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = degreeOfPrl });
      queuedCompletions.Add(sumBlk.Completion);

      // set up pipeline links between blocks
      taskSrcDataBlk.LinkTo(longTaskBlk, new DataflowLinkOptions { PropagateCompletion = true });
      longTaskBlk.LinkTo(sumBlk, new DataflowLinkOptions { PropagateCompletion = true });

      // produce items (add to queue)
      foreach(int number in numbers)
      {
        Console.WriteLine("adding {0} to queue", number);
        await taskSrcDataBlk.SendAsync(number);
      }
      taskSrcDataBlk.Complete();

      await Task.WhenAll(queuedCompletions);

      return sum;
    }


  }
}
