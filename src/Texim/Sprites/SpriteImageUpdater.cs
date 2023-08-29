// Copyright (c) 2023 SceneGate

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
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Texim.Pixels;
using Yarhl.FileFormat;

public class SpriteImageUpdater :
    IInitializer<SpriteImageUpdaterParams>,
    IConverter<ISprite, ISprite>
{
    private SpriteImageUpdaterParams parameters;

    public void Initialize(SpriteImageUpdaterParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        this.parameters = parameters;
    }

    public ISprite Convert(ISprite source)
    {
        ArgumentNullException.ThrowIfNull(source);

        foreach (var segment in source.Segments) {
            // No need to swizzle because the next methods retrieves the tiles in the same format from the image.
            IndexedPixel[] segmentTiles = parameters.Image.Pixels.GetSegmentPixels(segment, parameters.PixelsPerIndex);

            // Apply the flipping so it doesn't break the next algo searching the data.
            Span<IndexedPixel> spanSegment = segmentTiles;
            if (segment.HorizontalFlip) {
                spanSegment.FlipHorizontal(new Size(segment.Width, segment.Height));
            }

            if (segment.VerticalFlip) {
                spanSegment.FlipVertical(new Size(segment.Width, segment.Height));
            }

            // Now find unique pixels.
            // We don't need to find unique individual tiles but the full sequence of tiles of the OAM.
            // and put the start index in the OAM.
            AssignImageToSegment(segment, segmentTiles);
        }

        return source;
    }

    protected virtual void AssignImageToSegment(IImageSegment segment, IndexedPixel[] segmentTiles)
    {
        var segmentSize = new Size(segment.Width, segment.Height);
        var existingTiles = CollectionsMarshal.AsSpan(parameters.PixelSequences);
        var tileSearch = PixelSequenceFinder.SearchFlipping(existingTiles, segmentTiles, parameters.MinimumPixelsPerSegment, segmentSize);

        if (tileSearch.TileIdx == -1) {
            if (segmentTiles.Length < parameters.MinimumPixelsPerSegment) {
                int paddingPixelNum = parameters.MinimumPixelsPerSegment - segmentTiles.Length;
                segmentTiles = segmentTiles.Concat(new IndexedPixel[paddingPixelNum]).ToArray();
            }

            // Add sequence to the pixels.
            segment.TileIndex = parameters.PixelSequences.Count / parameters.PixelsPerIndex;
            parameters.PixelSequences.AddRange(segmentTiles);
        } else {
            segment.TileIndex = tileSearch.TileIdx / parameters.PixelsPerIndex;
        }

        segment.HorizontalFlip = tileSearch.HorizontalFlip;
        segment.VerticalFlip = tileSearch.VerticalFlip;
    }
}
