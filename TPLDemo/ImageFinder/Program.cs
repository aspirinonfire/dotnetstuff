using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFinder
{
  class Program
  {
    static void Main(string[] args)
    {
      List<Predicate<Image>> imageFilters;
      ImageProcessor imageProcessor = new ImageProcessor(Environment.ProcessorCount);

      string directory = @"C:\Users\Public\Pictures\Sample Pictures";
      imageFilters = new List<Predicate<Image>>();
      imageFilters.Add(ImageFilter.minResolutionFilterFactory(800, 600));
      imageFilters.Add(ImageFilter.aspectRatioFilterFactory(3.0/2));

      var task = imageProcessor.getMatchedImagesInDirectory(directory, imageFilters);
      task.Wait();
      foreach (string matchedPath in task.Result)
      {
        Console.WriteLine(matchedPath);
      }
      Console.WriteLine("====================");
      Console.WriteLine("Total matches: {0}", task.Result.Count());

      Console.ReadLine();
    }
  }
}
