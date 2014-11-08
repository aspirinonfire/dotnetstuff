using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using System.Threading;
using System.Drawing;
using System.IO;

namespace ImageFinder
{
  public class ImageProcessor
  {
    private struct Pipeline
    {
      public BufferBlock<string> producer { get; set; }
      public IEnumerable<Task> pipelineCompletions {get; set; }
    };

    private readonly int degreeOfParallelism;

    public ImageProcessor(int degreeOfParallelism)
    {
      this.degreeOfParallelism = degreeOfParallelism;
    }


    /// <summary>
    /// Produce basic image processor dataflow pipeline
    /// </summary>
    /// <param name="compositeImageFilter"></param>
    /// <param name="processorAction"></param>
    /// <returns></returns>
    private Pipeline pipelineFactory(Predicate<string> compositeImageFilter, Action<string> processorAction)
    {
      List<Task> pipelineCompletions = new List<Task>();

      // file path producer
      BufferBlock<string> producer = new BufferBlock<string>(new DataflowBlockOptions { BoundedCapacity = 100 });
      pipelineCompletions.Add(producer.Completion);

      // final consumer block
      ActionBlock<string> consumer = new ActionBlock<string>(
        (path) =>
        {
          processorAction.Invoke(path);
        },
        new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = degreeOfParallelism });
      pipelineCompletions.Add(consumer.Completion);

      // conditionally link producer and consumer on image filter results
      DataflowBlock.LinkTo(producer, consumer, new DataflowLinkOptions { PropagateCompletion = true }, compositeImageFilter);
      producer.LinkTo(DataflowBlock.NullTarget<string>());

      Pipeline pipeline = new Pipeline { pipelineCompletions = pipelineCompletions, producer = producer };

      return pipeline;
    }


    /// <summary>
    /// Combine multiple image filters into one AND filter
    /// </summary>
    /// <param name="imageFilters"></param>
    /// <returns></returns>
    private Predicate<string> compositeImageFilterFactory(IEnumerable<Predicate<Image>> imageFilters)
    {
      Predicate<string> compositeFilter =
      (path) =>
      {
        var img = Image.FromFile(path);

        return imageFilters.All(action => action.Invoke(img));
      };

      return compositeFilter;
    }


    /// <summary>
    /// Build and execute dataflow pipeline
    /// </summary>
    /// <param name="paths"></param>
    /// <param name="imageFilters"></param>
    /// <param name="processorAction"></param>
    /// <returns></returns>
    private async Task pipelineRunner(IEnumerable<string> paths, IEnumerable<Predicate<Image>> imageFilters, Action<string> processorAction)
    {
      var pipeline = pipelineFactory(compositeImageFilterFactory(imageFilters), processorAction);

      foreach (string path in paths)
      {
        await pipeline.producer.SendAsync(path);
      }
      pipeline.producer.Complete();
      await Task.WhenAll(pipeline.pipelineCompletions);
    }


    /// <summary>
    /// Return a collection of matched images
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="imageFilters"></param>
    /// <returns></returns>
    public async Task<IEnumerable<string>> getMatchedImagesInDirectory(string directory, IEnumerable<Predicate<Image>> imageFilters)
    {
      ConcurrentQueue<string> matches = new ConcurrentQueue<string>();

      Action<string> processorAction =
        (path) =>
        {
          matches.Enqueue(path);
        };

      string[] paths = Directory.GetFiles(directory, "*.jpg", SearchOption.AllDirectories);

      await pipelineRunner(paths, imageFilters, processorAction);

      return matches;
    }
  }
}
