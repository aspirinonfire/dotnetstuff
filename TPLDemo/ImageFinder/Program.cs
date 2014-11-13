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

      string srcDirectory = @"C:\Users\Public\Pictures\Sample Pictures";
      string dstDirectory = @"C:\Users\alexc.alexc-pc\Desktop\wallpaperworthy";
      imageFilters = new List<Predicate<Image>>()
      {
        ImageFilter.minResolutionFilterFactory(width: 800, height: 600),
        //ImageFilter.aspectRatioFilterFactory(horizontal: 16, vertical: 10, threshold: 0.01)
      };

      Console.WriteLine("Running image processor... this may take awhile");
      Console.WriteLine("Source: {0} \nDestination: {1}", srcDirectory, dstDirectory);
      //var task = imageProcessor.copyMatchingImagesAsync(srcDirectory, dstDirectory, imageFilters);
      //var task = imageProcessor.searchImagesAsync(srcDirectory, imageFilters);
      //task.Wait();

      Console.WriteLine("====================");
      Console.WriteLine("Total matches: {0}", imageProcessor.copyMatchingImages(srcDirectory, dstDirectory, imageFilters));

      Console.ReadLine();
    }
  }
}
