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
  /// <summary>
  /// Generic TPL Dataflow pipeline. Implements Producer-Consumer pattern
  /// Producer --> filter --> Consumer
  /// </summary>
  /// <typeparam name="In"></typeparam>
  public class LongTaskExecutor<In>
  {
    List<Task> queuedCompletions;

    private readonly BroadcastBlock<In> producerBlk;
    
    /// <summary>
    /// Create Long task executor object.
    /// This constructor creates and configures Dataflow pipeline
    /// </summary>
    /// <param name="maxInSrcQ">Max number of items that can be stored in queue at the same time</param>
    /// <param name="degreeOfPrl">Max number of threads that can be spawned to execute specific block</param>
    /// <param name="itemFilter">Produced item filter</param>
    /// <param name="finalConsumer">Consumer of filtered items</param>
    public LongTaskExecutor(int maxInSrcQ, int degreeOfPrl, Predicate<In> itemFilter, Action<In> finalConsumer)
    {
      queuedCompletions = new List<Task>();
      // this option required for propogating completion between linked blocks
      DataflowLinkOptions propogateCompletionOpt = new DataflowLinkOptions { PropagateCompletion = true };

      /**
       * Create and configure source block that will act as Producer.
       * This blocks serves as the entry point to a pipeline/network.
       */
      DataflowBlockOptions producerBlkOpts = new DataflowBlockOptions 
      { 
        // Max number of items that can exist in the queue at the same time
        BoundedCapacity = maxInSrcQ 
      };
      producerBlk = new BroadcastBlock<In>(item => item, producerBlkOpts);
      /**
       * add block completion state to a collection that will serve as indicator whether or not
       * this pipeline has finished processing all of the items
       */
      queuedCompletions.Add(producerBlk.Completion);

      /**
       * Create and configure action block that will act as a Consumer.
       */
      ExecutionDataflowBlockOptions consumerBlkOpts = new ExecutionDataflowBlockOptions
      {
        // max number of threads that a given block can be executed on
        MaxDegreeOfParallelism = degreeOfPrl
      };
      ActionBlock<In> consumerBlk = new ActionBlock<In>(
        (item) =>
        {
          Console.WriteLine("Consuming {0}", item);
          finalConsumer.Invoke(item);
        }, consumerBlkOpts);
      queuedCompletions.Add(consumerBlk.Completion);

      /**
       * Conditionally link Producer to a consumer
       * based on whether or not produced item passes a specified filter (predicate).
       * IMPORTANT: link producer to nullTarget so the item doesn't stay in the src queue
       * to avoid potential memory leaks if the item doesn't match filter.
       * This also prevents deadlocks.
       */
      DataflowBlock.LinkTo(producerBlk, consumerBlk, propogateCompletionOpt, itemFilter);
      producerBlk.LinkTo(DataflowBlock.NullTarget<In>());
    }


    /// <summary>
    /// Run items through the PTL Dataflow pipeline
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public async Task executePipeline(IEnumerable<In> items)
    {
      foreach (In item in items)
      {
        /**
         * Wait for an available slot and add an item to a source queue.
         * Number o slots is defined by BoundedCapacity in producerBlk.
         */
        await producerBlk.SendAsync(item);
        Console.WriteLine("Produced {0} on a queue", item);
      }
      /**
       * Notify producer block that no more items will be added to a queue.
       * This will set producer block state to complete and prevent consumers from consuming.
       */
      producerBlk.Complete();

      /**
       * Wait for all tasks spawned by the pipeline to complete
       */
      await Task.WhenAll(queuedCompletions);
    }
  }
}
