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
using Texim.Images;
using Texim.Pixels;

public static class SpriteImageExtensions
{
    public static FullImage SubImage(this IFullImage image, IImageSegment segment, SpriteRelativeCoordinatesKind relativeCoordinates)
    {
        (int relativeX, int relativeY) = relativeCoordinates switch {
            SpriteRelativeCoordinatesKind.TopLeft => (0, 0),
            SpriteRelativeCoordinatesKind.Center => (image.Width / 2, image.Height / 2),
            _ => throw new FormatException("Unknown relative position"),
        };

        return image.SubImage(
            segment.CoordinateX + relativeX,
            segment.CoordinateY + relativeY,
            segment.Width,
            segment.Height);
    }

    public static IndexedPixel[] GetSegmentPixels(this IndexedPixel[] source, IImageSegment segment, int pixelsPerIndex, int outOfBoundsTileIndex = -1)
    {
        var segmentPixels = new IndexedPixel[segment.Width * segment.Height];
        Span<IndexedPixel> pixelsOut = segmentPixels;
        Span<IndexedPixel> pixelsIn = source;

        int numPixels = segment.Width * segment.Height;

        int startIndex = segment.TileIndex * pixelsPerIndex;
        if (startIndex + numPixels > pixelsIn.Length || startIndex < 0) {
            if (outOfBoundsTileIndex == -1) {
                throw new FormatException($"Required tile index {segment.TileIndex} is out of bounds");
            } else {
                startIndex = outOfBoundsTileIndex * pixelsPerIndex;
            }
        }

        Span<IndexedPixel> tilesIn = pixelsIn.Slice(startIndex, numPixels);
        for (int t = 0; t < numPixels; t++) {
            pixelsOut[t] = new IndexedPixel(
                tilesIn[t].Index,
                tilesIn[t].Alpha,
                segment.PaletteIndex);
        }

        return segmentPixels;
    }
}
