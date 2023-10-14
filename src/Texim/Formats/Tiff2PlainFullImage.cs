namespace Texim.Formats;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texim.Colors;
using Texim.Images;
using Texim.Pixels;
using Yarhl.FileFormat;

public class Tiff2PlainFullImage : IConverter<TiffImage, FullImage>
{
    public FullImage Convert(TiffImage source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var image = new FullImage(source.CanvasWidth, source.CanvasHeight);
        foreach (TiffPage page in source.Pages.Reverse()) {
            DrawLayer(page, image);
        }

        return image;
    }

    private static void DrawLayer(TiffPage page, FullImage image)
    {
        for (int x = 0; x < page.Width; x++) {
            for (int y = 0; y < page.Height; y++) {
                Rgb pixel = GetColor(page, x, y);
                if (pixel.Alpha == 0) {
                    continue; // don't overwrite other layers.
                }

                int imageIdx = ((page.Y + y) * image.Width) + page.X + x;
                image.Pixels[imageIdx] = pixel;
            }
        }
    }

    private static Rgb GetColor(TiffPage page, int x, int y)
    {
        int idx = (y * page.Width) + x;
        if (page.IsIndexed) {
            IndexedPixel pixel = page.IndexedPixels[idx];
            return new Rgb(page.ColorMap[pixel.Index], pixel.Alpha);
        }

        return page.RgbPixels[idx];
    }
}
