using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageFinder
{
  public class ImageFilter
  {
    public static Predicate<Image> minResolutionFilterFactory(int width, int height)
    {
      Predicate<Image> filter =
        (img) =>
        {
          return img.Height >= height && img.Width >= width;
        };

      return filter;
    }

    public static Predicate<Image> aspectRatioFilterFactory(int horizontal, int vertical, double threshold = 0.01)
    {
      double ratio = (double)horizontal / vertical;
      Predicate<Image> filter =
        (img) =>
        {
          return Math.Abs((double)img.Width / img.Height - ratio) <= threshold;
        };

      return filter;
    }

    public static Predicate<Image> acceptAllFilterFactory()
    {
      Predicate<Image> filter = (img) => true;
      return filter;
    }
  }
}
