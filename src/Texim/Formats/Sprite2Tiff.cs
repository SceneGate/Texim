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
namespace Texim.Formats;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Texim.Colors;
using Texim.Palettes;
using Texim.Pixels;
using Texim.Sprites;
using Yarhl.FileFormat;

public class Sprite2Tiff : IConverter<ISprite, TiffImage>
{
    private readonly Sprite2TiffParams parameters;

    private readonly List<IImageSegment> layerSegments = new List<IImageSegment>();
    private Rectangle layerBounds;
    private int layerPaletteIdx;

    public Sprite2Tiff(Sprite2TiffParams parameters)
    {
        this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    public TiffImage Convert(ISprite source)
    {
        ArgumentNullException.ThrowIfNull(source);

        (int relativeX, int relativeY) = parameters.RelativeCoordinates switch {
            SpriteRelativeCoordinatesKind.TopLeft => (0, 0),
            SpriteRelativeCoordinatesKind.Center => (source.Width / 2, source.Height / 2),
            _ => throw new FormatException("Unknown relative position"),
        };

        var orderedSegments = source.Segments
            .Select((x, idx) => new { Id = idx, Seg = x })
            .OrderByDescending(x => x.Seg.Layer)
            .ThenByDescending(x => x.Id)
            .Select(x => x.Seg)
            .ToArray();

        var segmentGroups = new List<(IImageSegment[] Segments, Rectangle Bounds)>();
        if (parameters.ReduceLayers) {
            foreach (var segment in orderedSegments) {
                if (layerSegments.Count == 0 || !AddSegmentIfSameLayer(segment)) {
                    if (layerSegments.Count > 0) {
                        segmentGroups.Add((layerSegments.ToArray(), layerBounds));
                        layerSegments.Clear();
                    }

                    layerSegments.Add(segment);

                    layerBounds = new Rectangle(segment.CoordinateX, segment.CoordinateY, segment.Width, segment.Height);
                    layerPaletteIdx = segment.PaletteIndex;
                }
            }

            if (layerSegments.Count > 0) {
                segmentGroups.Add((layerSegments.ToArray(), layerBounds));
            }
        } else {
            foreach (var segment in orderedSegments) {
                segmentGroups.Add((new[] { segment }, CreateRectangle(segment)));
            }
        }

        var sprite2Image = new Sprite2IndexedImage();
        sprite2Image.Initialize(new Sprite2IndexedImageParams {
            FullImage = parameters.Image,
            IsTiled = parameters.IsTiled,
            TileSize = parameters.TileSize,
            OutOfBoundsTileIndex = -1,
            RelativeCoordinates = SpriteRelativeCoordinatesKind.Reset,
        });

        Rgb[] fullPalette = parameters.Palettes.Palettes.SelectMany(p => p.Colors).ToArray();

        var tiff = new TiffImage();
        tiff.CanvasWidth = source.Width;
        tiff.CanvasHeight = source.Height;

        for (int i = 0; i < segmentGroups.Count; i++) {
            var layerSprite = new Sprite {
                Width = segmentGroups[i].Bounds.Width,
                Height = segmentGroups[i].Bounds.Height,
                Segments = new Collection<IImageSegment>(segmentGroups[i].Segments),
            };
            var layerIndexedImage = sprite2Image.Convert(layerSprite);

            var page = new TiffPage {
                Id = i,
                X = segmentGroups[i].Bounds.Left + relativeX,
                Y = segmentGroups[i].Bounds.Top + relativeY,
                Width = layerIndexedImage.Width,
                Height = layerIndexedImage.Height,
            };
            tiff.Pages.Add(page);

            if (parameters.ExportAsIndexedImage) {
                ConvertToFullIndex(layerIndexedImage.Pixels, parameters.Palettes);
                page.IndexedPixels = layerIndexedImage.Pixels;
                page.ColorMap = fullPalette;
                page.IsIndexed = true;
            } else {
                page.RgbPixels = layerIndexedImage.CreateFullImage(parameters.Palettes).Pixels;
            }
        }

        return tiff;
    }

    private static void ConvertToFullIndex(IndexedPixel[] pixels, IPaletteCollection palettes)
    {
        for (int i = 0; i < pixels.Length; i++) {
            int paletteIdx = palettes.Palettes.Take(pixels[i].PaletteIndex).Sum(p => p.Colors.Count);
            int pixelIdx = paletteIdx + pixels[i].Index;

            pixels[i] = new IndexedPixel((short)pixelIdx, pixels[i].Alpha, 0);
        }
    }

    private static Rectangle CreateRectangle(IImageSegment segment) =>
        new Rectangle(segment.CoordinateX, segment.CoordinateY, segment.Width, segment.Height);

    private bool AddSegmentIfSameLayer(IImageSegment segment)
    {
        if (layerPaletteIdx != segment.PaletteIndex) {
            return false;
        }

        var segmentBounds = CreateRectangle(segment);
        if (IntersectsWithLayer(segmentBounds)) {
            return false;
        }

        if (!IsAdjacentWithLayer(segmentBounds)) {
            return false;
        }

        layerSegments.Add(segment);

        int xMin = segmentBounds.Left < layerBounds.Left ? segmentBounds.Left : layerBounds.Left;
        int xMax = segmentBounds.Right > layerBounds.Right ? segmentBounds.Right : layerBounds.Right;
        int yMin = segmentBounds.Top < layerBounds.Top ? segmentBounds.Top : layerBounds.Top;
        int yMax = segmentBounds.Bottom > layerBounds.Bottom ? segmentBounds.Bottom : layerBounds.Bottom;
        layerBounds = new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);

        return true;
    }

    private bool IntersectsWithLayer(Rectangle segmentBounds)
    {
        return layerSegments.Exists(s => CreateRectangle(s).IntersectsWith(segmentBounds));
    }

    private bool IsAdjacentWithLayer(Rectangle segmentBounds)
    {
        static bool IsAdjacent(Rectangle rect1, Rectangle rect2)
        {
            if ((rect1.Left == rect2.Right) || (rect1.Right == rect2.Left)) {
                return ((rect1.Top <= rect2.Top) && (rect1.Bottom >= rect2.Bottom)) ||
                    ((rect2.Top <= rect1.Top) && (rect2.Bottom >= rect1.Bottom));
            }

            if ((rect1.Top == rect2.Bottom) || (rect1.Bottom == rect2.Top)) {
                return ((rect1.Left <= rect2.Left) && (rect1.Right >= rect2.Right)) ||
                    ((rect2.Left <= rect1.Left) && (rect2.Right >= rect1.Right));
            }

            return false;
        }

        return layerSegments.Exists(s => IsAdjacent(CreateRectangle(s), segmentBounds));
    }
}
