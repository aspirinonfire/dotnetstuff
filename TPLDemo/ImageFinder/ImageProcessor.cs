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
    private Pipeline pipelineFactory(IEnumerable<Predicate<Image>> imageFilters, Action<string> processorAction)
    {
      List<Task> pipelineCompletions = new List<Task>();
      var execOpts = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = degreeOfParallelism };
      var linkOpts = new DataflowLinkOptions { PropagateCompletion = true }; 

      // file path producer
      BufferBlock<string> producer = new BufferBlock<string>(new DataflowBlockOptions { BoundedCapacity = 100 });
      pipelineCompletions.Add(producer.Completion);

      // filter transform block
      TransformBlock<string, string> filterBlk = new TransformBlock<string, string>(
        (path) =>
        {
          using (Image img = Image.FromFile(path))
          {
            bool takeImage = imageFilters.All(filter => filter.Invoke(img));
            return takeImage ? path : null;
          }
        },
        execOpts);
      pipelineCompletions.Add(filterBlk.Completion);

      // final consumer block
      ActionBlock<string> consumer = new ActionBlock<string>(
        (path) =>
        {
          processorAction.Invoke(path);
        },
        execOpts);
      pipelineCompletions.Add(consumer.Completion);

      // link blocks
      producer.LinkTo(filterBlk, linkOpts);
      DataflowBlock.LinkTo(filterBlk, consumer, linkOpts, (path) => !String.IsNullOrWhiteSpace(path));
      filterBlk.LinkTo(DataflowBlock.NullTarget<string>());

      Pipeline pipeline = new Pipeline { pipelineCompletions = pipelineCompletions, producer = producer };
      return pipeline;
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
      var pipeline = pipelineFactory(imageFilters, processorAction);

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
