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
namespace Texim.Games.Nitro;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Texim.Colors;
using Texim.Formats;
using Texim.Images;
using Texim.Pixels;
using Texim.Processing;
using Texim.Sprites;
using Yarhl.FileFormat;

public class FullImage2ReferenceNitroCell :
    IConverter<FullImage, ISprite>,
    IInitializer<FullImage2ReferenceNitroCellParams>
{
    private FullImage2ReferenceNitroCellParams parameters;

    public void Initialize(FullImage2ReferenceNitroCellParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        this.parameters = parameters;
    }

    public ISprite Convert(FullImage source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(parameters);

        var newSprite = new Cell(parameters.ReferenceCell, Array.Empty<IImageSegment>()) {
            Width = parameters.ReferenceCell.Width,
            Height = parameters.ReferenceCell.Height,
        };

        // Clone the image as we will modifying it.
        FullImage newImage = new FullImage(source.Width, source.Height) {
            Pixels = source.Pixels.ToArray(),
        };

        // Gets the original final image, so we can compare overlayed pixels
        var sprite2Image = new Sprite2IndexedImage();
        sprite2Image.Initialize(new Sprite2IndexedImageParams {
            FullImage = parameters.Image,
            IsTiled = parameters.IsImageTiled,
            RelativeCoordinates = parameters.RelativeCoordinates,
        });
        FullImage originalSpriteImage = sprite2Image.Convert(parameters.ReferenceCell)
            .CreateFullImage(parameters.Palettes);

        ImageSegment2IndexedImage segment2Indexed = new ImageSegment2IndexedImage();
        segment2Indexed.Initialize(new ImageSegment2IndexedImageParams {
            FullImage = parameters.Image,
            IsTiled = parameters.IsImageTiled,
        });

        // Start with the segments of the reference cell.
        // We need to import from top to down drawing order as we will referencing the imported pixels from up layers.
        var orderedSegments = parameters.ReferenceCell.Segments
            .Select((x, idx) => new { Id = idx, Seg = x })
            .OrderBy(x => x.Seg.Layer)
            .ThenBy(x => x.Id)
            .Select(x => x.Seg)
            .ToArray();
        for (int i = 0; i < orderedSegments.Length; i++) {
            IImageSegment segment = orderedSegments[i];

            // Get the original and new full image VISIBLE segments.
            // if there are sections overlayed with another layer then we don't care about those pixels.
            FullImage originalLayeredSegmentImage = originalSpriteImage.SubImage(segment, parameters.RelativeCoordinates);
            FullImage originalSegmentImage = segment2Indexed.Convert(segment).CreateFullImage(parameters.Palettes, true);
            FullImage segmentImage = PopVisiblePixels(
                source,
                newImage,
                parameters.PixelSequences,
                originalLayeredSegmentImage,
                originalSegmentImage,
                segment,
                newSprite.Segments.Take(i).ToArray(),
                parameters.RelativeCoordinates,
                parameters.PixelsPerIndex);

            // If our imported image has the same pixels as the original...
            IndexedPixel[] segmentTiles;
            byte paletteIdx;
            if (HasSameVisiblePixels(segmentImage, originalSegmentImage)) {
                // ... then we use the original tiles and palette.
                segmentTiles = parameters.Image.Pixels.GetSegmentPixels(segment, parameters.PixelsPerIndex);
                paletteIdx = segment.PaletteIndex;
            } else {
                if (!string.IsNullOrEmpty(parameters.DebugNewLayersPath)) {
                    var image2Bitmap = new FullImage2Bitmap();
                    image2Bitmap.Convert(originalLayeredSegmentImage).Stream.WriteTo($"{parameters.DebugNewLayersPath}_{i}_originalLayered.png");
                    image2Bitmap.Convert(originalSegmentImage).Stream.WriteTo($"{parameters.DebugNewLayersPath}_{i}_originalSegment.png");
                    image2Bitmap.Convert(segmentImage).Stream.WriteTo($"{parameters.DebugNewLayersPath}_{i}_new.png");
                }

                // ... if they don't, then we quantize the new segment.
                var quantization = new FixedPaletteTileQuantization(
                    parameters.Palettes,
                    new Size(segment.Width, segment.Height),
                    segment.Width);
                quantization.FirstAsTransparent = true;
                var quantizationResult = (quantization.Quantize(segmentImage.Pixels) as FixedPaletteTileQuantizationResult)!;

                paletteIdx = quantizationResult.PaletteIndexes[0];
                if (parameters.IsImageTiled) {
                    var segmentSwizzler = new TileSwizzling<IndexedPixel>(segment.Width);
                    segmentTiles = segmentSwizzler.Swizzle(quantizationResult.Pixels);
                } else {
                    segmentTiles = quantizationResult.Pixels;
                }
            }

            // Now let's put our tiles into the unique image segments pool.
            var newSegment = AssignImageToSegment(segment, segmentTiles, paletteIdx);
            newSprite.Segments.Add(newSegment);
        }

        // At this point, every non-transparent pixel is for new segments.
        // so let's segment the new area.
        (Sprite newSegmentsSprite, _) = parameters.Segmentation.Segment(newImage);

        foreach (var newSegment in newSegmentsSprite.Segments) {
            FullImage segmentImage = source.SubImage(newSegment, parameters.RelativeCoordinates);

            var quantization = new FixedPaletteTileQuantization(
                parameters.Palettes,
                new Size(newSegment.Width, newSegment.Height),
                newSegment.Width);
            quantization.FirstAsTransparent = true;
            var quantizationResult = (quantization.Quantize(segmentImage.Pixels) as FixedPaletteTileQuantizationResult)!;

            IndexedPixel[] segmentTiles;
            if (parameters.IsImageTiled) {
                var segmentSwizzler = new TileSwizzling<IndexedPixel>(newSegment.Width);
                segmentTiles = segmentSwizzler.Swizzle(quantizationResult.Pixels);
            } else {
                segmentTiles = quantizationResult.Pixels;
            }

            var newSegmentWithTile = AssignImageToSegment(newSegment, segmentTiles, quantizationResult.PaletteIndexes[0]);
            newSprite.Segments.Add(newSegmentWithTile);
        }

        return newSprite;
    }

    private FullImage PopVisiblePixels(
        FullImage image,
        FullImage processedImage,
        List<IndexedPixel> segmentsPixelBuffer,
        FullImage referenceImage,
        FullImage referenceSegmentImage,
        IImageSegment segment,
        IImageSegment[] topSegments,
        SpriteRelativeCoordinatesKind relativeCoordinates,
        int pixelsPerIndex)
    {
        (int relativeX, int relativeY) = relativeCoordinates switch {
            SpriteRelativeCoordinatesKind.TopLeft => (0, 0),
            SpriteRelativeCoordinatesKind.Center => (image.Width / 2, image.Height / 2),
            _ => throw new FormatException("Unknown relative position"),
        };

        int startX = segment.CoordinateX + relativeX;
        int startY = segment.CoordinateY + relativeY;
        var subImage = new FullImage(segment.Width, segment.Height);

        CompareNonTransparentPixels nonTransparentComparer = new();

        int idx = 0;
        for (int y = 0; y < segment.Height; y++) {
            for (int x = 0; x < segment.Width; x++) {
                int segmentIdx = idx;
                int fullIndex = ((startY + y) * image.Width) + startX + x;

                int canvasX = x + segment.CoordinateX;
                int canvasY = y + segment.CoordinateY;
                if (IsPixelFromTopSegments(canvasX, canvasY, topSegments, segmentsPixelBuffer, pixelsPerIndex)) {
                    // As it's hidden, take the original color
                    // we could also take the one from the image setting alpha to 0
                    subImage.Pixels[idx++] = referenceSegmentImage.Pixels[segmentIdx];
                } else if (nonTransparentComparer.Equals(image.Pixels[fullIndex], referenceImage.Pixels[segmentIdx])) {
                    // This allows to set it as transparent as the original
                    // in case the color we get from the imported image comes from the bottom layer.
                    // If the overlayed images are same, take the color from drawing only the segment (no overlays)
                    subImage.Pixels[idx++] = referenceSegmentImage.Pixels[segmentIdx];
                } else if (!parameters.ImportTopToBottom
                    && referenceImage.Pixels[segmentIdx].Alpha != 0 && referenceSegmentImage.Pixels[segmentIdx].Alpha == 0) {
                    // If the original pixel with overlays is NOT transparent
                    // but drawing only this segment (without overlays) is transparent.
                    // then the pixel of our image is coming from a bottom layer, so ignore it for now.
                    // BUT this has the side effect that if the NEW top layer has new pixels in transparent areas...
                    // ... they are going to be put in the bottom layer which before was clean...
                    subImage.Pixels[idx++] = referenceSegmentImage.Pixels[segmentIdx];
                } else {
                    subImage.Pixels[idx++] = image.Pixels[fullIndex];
                }

                // Set processed pixels as transparent
                processedImage.Pixels[fullIndex] = new Rgb(22, 8, 93, 0);
            }
        }

        return subImage;
    }

    private bool IsPixelFromTopSegments(int x, int y, IEnumerable<IImageSegment> segments, List<IndexedPixel> segmentsPixelBuffer, int pixelsPerIndex)
    {
        bool IsPixelInLayer(IImageSegment segment) =>
            x >= segment.CoordinateX
            && x < segment.CoordinateX + segment.Width
            && y >= segment.CoordinateY
            && y < segment.CoordinateY + segment.Height;

        bool IsPixelVisible(IImageSegment segment)
        {
            // Another option would be to keep all the segment FullImage and compare against...
            int startIndex = segment.TileIndex * pixelsPerIndex;
            int segmentX = x - segment.CoordinateX;
            int segmentY = y - segment.CoordinateY;
            int index = startIndex + (segmentY * segment.Width) + segmentX;

            return (segmentsPixelBuffer[index].Alpha != 0) && (segmentsPixelBuffer[index].Index > 0);
        }

        return segments.Any(seg => IsPixelInLayer(seg) && IsPixelVisible(seg));
    }

    private bool HasSameVisiblePixels(FullImage img1, FullImage img2)
    {
        return img1.Pixels.SequenceEqual(img2.Pixels, new CompareNonTransparentPixels());
    }

    private IImageSegment AssignImageToSegment(IImageSegment segmentStructure, IndexedPixel[] segmentTiles, byte paletteIndex)
    {
        var newSegment = new ObjectAttributeMemory(segmentStructure);

        // We only support one palette per segment.
        newSegment.PaletteIndex = paletteIndex;

        var segmentSize = new Size(segmentStructure.Width, segmentStructure.Height);
        var existingTiles = CollectionsMarshal.AsSpan(parameters.PixelSequences);
        var tileSearch = PixelSequenceFinder.SearchFlipping(existingTiles, segmentTiles, parameters.MinimumPixelsPerSegment, segmentSize);
        if (tileSearch.TileIdx == -1) {
            if (segmentTiles.Length < parameters.MinimumPixelsPerSegment) {
                int paddingPixelNum = parameters.MinimumPixelsPerSegment - segmentTiles.Length;
                segmentTiles = segmentTiles.Concat(new IndexedPixel[paddingPixelNum]).ToArray();
            }

            // Add sequence to the pixels.
            newSegment.TileIndex = parameters.PixelSequences.Count / parameters.PixelsPerIndex;
            parameters.PixelSequences.AddRange(segmentTiles);
        } else {
            newSegment.TileIndex = tileSearch.TileIdx / parameters.PixelsPerIndex;
        }

        newSegment.HorizontalFlip = tileSearch.HorizontalFlip;
        newSegment.VerticalFlip = tileSearch.VerticalFlip;

        newSegment.PaletteMode = parameters.Has8bppDepth
                ? NitroPaletteMode.Palette256x1
                : NitroPaletteMode.Palette16x16;

        return newSegment;
    }

    private sealed class CompareNonTransparentPixels : IEqualityComparer<Rgb>
    {
        public bool Equals(Rgb x, Rgb y) =>
            (x.Alpha == 0 && y.Alpha == 0) || x.Equals(y);

        public int GetHashCode([DisallowNull] Rgb obj)
        {
            if (obj.Alpha == 0) {
                return HashCode.Combine(obj.Alpha);
            }

            return obj.GetHashCode();
        }
    }
}
