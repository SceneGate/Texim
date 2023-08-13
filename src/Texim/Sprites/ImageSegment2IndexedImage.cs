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
using Texim.Images;
using Texim.Pixels;
using Yarhl.FileFormat;

public class ImageSegment2IndexedImage :
    IInitializer<ImageSegment2IndexedImageParams>,
    IConverter<IImageSegment, IndexedImage>
{
    private ImageSegment2IndexedImageParams parameters;

    public void Initialize(ImageSegment2IndexedImageParams parameters)
    {
        this.parameters = parameters;
    }

    public IndexedImage Convert(IImageSegment source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (parameters is null)
            throw new InvalidOperationException("Missing initialization");

        // If it was tiled swizzled, then convert to tiles so we can resolve the references
        IndexedPixel[] tiles = parameters.FullImage.Pixels;
        if (parameters.IsTiled) {
            var swizzling = new TileSwizzling<IndexedPixel>(parameters.TileSize, parameters.FullImage.Width);
            tiles = swizzling.Swizzle(parameters.FullImage.Pixels);
        }

        int pixelsPerIndex = parameters.TileSize.Width * parameters.TileSize.Height;
        var segmentTiles = tiles.GetSegmentPixels(source, pixelsPerIndex, parameters.OutOfBoundsTileIndex);

        // Unswizzle but this time with the final image size
        if (parameters.IsTiled) {
            var unswizzling = new TileSwizzling<IndexedPixel>(parameters.TileSize, source.Width);
            segmentTiles = unswizzling.Unswizzle(segmentTiles);
        }

        // We flip treating the full image unswizzle as a tile
        Span<IndexedPixel> spanSegment = segmentTiles;
        if (source.HorizontalFlip) {
            spanSegment.FlipHorizontal(new System.Drawing.Size(source.Width, source.Height));
        }

        if (source.VerticalFlip) {
            spanSegment.FlipVertical(new System.Drawing.Size(source.Width, source.Height));
        }

        return new IndexedImage {
            Height = source.Height,
            Width = source.Width,
            Pixels = segmentTiles,
        };
    }
}
