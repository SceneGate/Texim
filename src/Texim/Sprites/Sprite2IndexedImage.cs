// Copyright (c) 2022 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace Texim.Sprites;

using System;
using System.Linq;
using Texim.Images;
using Texim.Pixels;
using Yarhl.FileFormat;

public class Sprite2IndexedImage :
    IInitializer<ImageSegment2IndexedImageParams>,
    IConverter<ISprite, IndexedImage>
{
    private readonly ImageSegment2IndexedImage segmentConverter = new ();
    private ImageSegment2IndexedImageParams parameters;

    public void Initialize(ImageSegment2IndexedImageParams parameters)
    {
        this.parameters = parameters;
        segmentConverter.Initialize(parameters);
    }

    public IndexedImage Convert(ISprite source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (parameters is null)
            throw new InvalidOperationException("Missing initialization");

        var pixels = new IndexedPixel[source.Width * source.Height];
        Span<IndexedPixel> pixelsSpan = pixels;

        int centerX = source.Width / 2;
        int centerY = source.Height / 2;
        foreach (var segment in source.Segments.OrderBy(s => s.Layer)) {
            CopySegment(segment, pixelsSpan, source.Width, centerX, centerY);
        }

        var image = new IndexedImage {
            Pixels = pixels,
            Width = source.Width,
            Height = source.Height,
        };

        return image;
    }

    private void CopySegment(IImageSegment segment, Span<IndexedPixel> output, int width, int centerX, int centerY)
    {
        IndexedImage segmentImage = segmentConverter.Convert(segment);

        for (int x = 0; x < segment.Width; x++) {
            for (int y = 0; y < segment.Height; y++) {
                int inIdx = (y * segment.Width) + x;
                IndexedPixel pixel = segmentImage.Pixels[inIdx];
                if (pixel.Alpha == 0 || pixel.Index == 0) {
                    continue;
                }

                int outIdx = ((centerY + segment.CoordinateY + y) * width) + (centerX + segment.CoordinateX + x);
                output[outIdx] = pixel;
            }
        }
    }
}
