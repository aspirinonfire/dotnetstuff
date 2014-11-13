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

    private delegate Task ConsumerAction(FileStream src);

    private readonly int degreeOfParallelism;

    public ImageProcessor(int degreeOfParallelism)
    {
      this.degreeOfParallelism = degreeOfParallelism;
    }


    /// <summary>
    /// Produce basic image processor dataflow pipeline.
    /// producer -> pathToStreamBlk -> imageFilterBlk -> consumer
    /// </summary>
    /// <param name="compositeImageFilter"></param>
    /// <param name="consumerAction"></param>
    /// <returns></returns>
    private Pipeline pipelineFactory(IEnumerable<Predicate<Image>> imageFilters, ConsumerAction consumerAction)
    {
      List<Task> pipelineCompletions = new List<Task>();
      var execOpts = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = degreeOfParallelism };
      var linkOpts = new DataflowLinkOptions { PropagateCompletion = true }; 

      // file path producer
      BufferBlock<string> producer = new BufferBlock<string>(new DataflowBlockOptions { BoundedCapacity = 100 });
      pipelineCompletions.Add(producer.Completion);

      //  open stream from path block
      TransformBlock<string, FileStream> pathToStreamBlk = new TransformBlock<string, FileStream>(
        (path) =>
        {
          FileStream sourceStream = null;
          try
          {
            // open file stream in async mode
            sourceStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true);
          }
          catch (Exception ex)
          {
            // silently drop exceptions and treat path as bad image
          }
          return sourceStream;
        },
        new ExecutionDataflowBlockOptions 
        { 
          MaxDegreeOfParallelism = degreeOfParallelism,
        });
      producer.LinkTo(pathToStreamBlk, linkOpts);
      pipelineCompletions.Add(pathToStreamBlk.Completion);

      // image filter transform block
      TransformBlock<FileStream, FileStream> imageFilterBlk = new TransformBlock<FileStream, FileStream>(
        (src) =>
        {
          bool takeImage = false;

          try
          {
            using (Image img = Image.FromStream(src))
            {
              takeImage = imageFilters.All(filter => filter.Invoke(img));
            }
            src.Seek(0, SeekOrigin.Begin);
          }
          catch (Exception ex)
          {
            // silently drop exceptions and treat path as bad image
          }
          
          return takeImage ? src : null;
        },
        execOpts);
      DataflowBlock.LinkTo(pathToStreamBlk, imageFilterBlk, linkOpts, (src) => src != null);
      pathToStreamBlk.LinkTo(DataflowBlock.NullTarget<FileStream>());
      pipelineCompletions.Add(imageFilterBlk.Completion);

      // final consumer block
      ActionBlock<FileStream> consumer = new ActionBlock<FileStream>(
        async (src) =>
        {
          await consumerAction.Invoke(src);
          src.Close();
          src.Dispose();
        },
        execOpts);
      DataflowBlock.LinkTo(imageFilterBlk, consumer, linkOpts, (src) => src != null);
      imageFilterBlk.LinkTo(DataflowBlock.NullTarget<FileStream>());
      pipelineCompletions.Add(consumer.Completion);

      Pipeline pipeline = new Pipeline { pipelineCompletions = pipelineCompletions, producer = producer };
      return pipeline;
    }


    /// <summary>
    /// Build and execute dataflow pipeline
    /// </summary>
    /// <param name="paths"></param>
    /// <param name="imageFilters"></param>
    /// <param name="consumerAction"></param>
    /// <returns></returns>
    private async Task pipelineRunner(IEnumerable<string> paths, IEnumerable<Predicate<Image>> imageFilters, ConsumerAction consumerAction)
    {
      var pipeline = pipelineFactory(imageFilters, consumerAction);

      foreach (string path in paths)
      {
        await pipeline.producer.SendAsync(path);
      }
      pipeline.producer.Complete();
      await Task.WhenAll(pipeline.pipelineCompletions);
    }


    /// <summary>
    /// Search, filter and apply action to images
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="imageFilters"></param>
    /// <param name="matchAction"></param>
    private void processImages(IEnumerable<string> paths, IEnumerable<Predicate<Image>> imageFilters, Action<string> matchAction)
    {
      var results = Parallel.ForEach(paths, new ParallelOptions { MaxDegreeOfParallelism = this.degreeOfParallelism }, 
        (path) =>
        {
          try
          {
            using (Image img = Image.FromFile(path))
            {
              if (imageFilters.All(filter => filter.Invoke(img)))
              {
                matchAction.Invoke(path);
              }
            }
          }
          catch (Exception ex)
          {

          }
        });
    }


    /// <summary>
    /// Get a collection of image files in specified directory
    /// </summary>
    /// <param name="srcDirectory"></param>
    /// <returns></returns>
    private IEnumerable<string> getImageFiles(string srcDirectory)
    {
      return Directory.EnumerateFiles(srcDirectory, "*.jpg", SearchOption.AllDirectories);
    }


    /// <summary>
    /// Generate destination filename
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="srcFilename"></param>
    /// <returns></returns>
    private string generateFileName(string destination, string srcFilename)
    {
      string newFile;
      if (File.Exists(Path.Combine(destination, srcFilename)))
      {
        StringBuilder filenameBuilder = new StringBuilder();

        string[] nameParts = srcFilename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

        filenameBuilder
          .Append(nameParts[0])
          .Append("_")
          .Append(DateTime.Now.ToString("yyyyMMddHHmmssfff"))
          .Append(".")
          .Append(nameParts[1]);

        newFile = Path.Combine(destination, filenameBuilder.ToString());
      }
      else
      {
        newFile = Path.Combine(destination, srcFilename);
      }

      return newFile;
    }


    /// <summary>
    /// Copy images that match filters 
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="destination"></param>
    /// <param name="imageFilters"></param>
    /// <returns></returns>
    public int copyMatchingImages(string directory, string destination, IEnumerable<Predicate<Image>> imageFilters)
    {
      int matchedFiles = 0;
      destination = Path.Combine(destination, "img_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
      bool destinationExists = Directory.Exists(destination);

      // define action that copies matched images to a destination
      Action<string> matchAction =
        (path) =>
        {
          string srcFilename = path.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Last();

          // check if destination file exists
          string newFile = generateFileName(destination, srcFilename);

          if (!destinationExists)
          {
            Directory.CreateDirectory(destination);
            destinationExists = true;
          }

          File.Copy(path, newFile);

          Console.WriteLine("Copied {0}", srcFilename);
          Interlocked.Increment(ref matchedFiles);
        };


      processImages(getImageFiles(directory), imageFilters, matchAction);

      return matchedFiles;
    }


    /// <summary>
    /// Copy images that match filters asynchronously
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="destination"></param>
    /// <param name="imageFilters"></param>
    /// <returns>Number of matched images</returns>
    public async Task<int> copyMatchingImagesAsync(string directory, string destination, IEnumerable<Predicate<Image>> imageFilters)
    {
      int matchedFiles = 0;
      destination = Path.Combine(destination, "img_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
      bool destinationExists = Directory.Exists(destination);

      // define action that copies matched images to a destination
      ConsumerAction consumerAction =
        async (src) =>
        {
          string srcFilename = src.Name.Split(new string[]{"\\"}, StringSplitOptions.RemoveEmptyEntries).Last();

          // check if destination file exists
          string newFile = generateFileName(destination, srcFilename);

          if (!destinationExists)
          {
            Directory.CreateDirectory(destination);
            destinationExists = true;
          }

          // copy image to a new location
          using (FileStream dst = File.Create(newFile))
          {
            await src.CopyToAsync(dst);
            await dst.FlushAsync();
          }

          Console.WriteLine("Copied {0}", srcFilename);
          Interlocked.Increment(ref matchedFiles);
        };


      await pipelineRunner(getImageFiles(directory), imageFilters, consumerAction);

      return matchedFiles;
    }


    /// <summary>
    /// Search images that match supplied filters
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="imageFilters"></param>
    /// <returns></returns>
    public IEnumerable<string> searchImages(string directory, IEnumerable<Predicate<Image>> imageFilters)
    {
      ConcurrentQueue<string> matches = new ConcurrentQueue<string>();

      Action<string> matchAction =
        (path) =>
        {
          matches.Enqueue(path);
        };

      processImages(getImageFiles(directory), imageFilters, matchAction);

      return matches;
    }


    /// <summary>
    /// Search images that match supplied filters asynchronously
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="imageFilters"></param>
    /// <returns>Collection of image paths</returns>
    public async Task<IEnumerable<string>> searchImagesAsync(string directory, IEnumerable<Predicate<Image>> imageFilters)
    {
      ConcurrentQueue<string> matches = new ConcurrentQueue<string>();

      ConsumerAction consumerAction =
        (src) =>
        {
          // This is actually bad, sync code shouldn't be wrapped async...
          // But I did it anyways to match delegate signature
          return Task.Factory.StartNew(() => matches.Enqueue(src.Name));
        };

      await pipelineRunner(getImageFiles(directory), imageFilters, consumerAction);

      return matches;
    }
  }
}
