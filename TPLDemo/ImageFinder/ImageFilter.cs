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

    public static Predicate<Image> aspectRatioFilterFactory(double ratio, double ratioThreshold = 0.01)
    {
      Predicate<Image> filter =
        (img) =>
        {
          return Math.Abs((double)img.Width / img.Height - ratio) <= ratioThreshold;
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
