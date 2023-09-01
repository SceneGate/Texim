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
using System.IO;
using System.Linq;
using System.Text;
using BitMiracle.LibTiff.Classic;
using Texim.Colors;
using Texim.Images;
using Texim.Pixels;
using Texim.Sprites;
using Yarhl.FileFormat;
using Yarhl.IO;

public class Sprite2Tiff : IConverter<ISprite, BinaryFormat>
{
    private readonly Sprite2TiffParams parameters;

    private readonly List<IImageSegment> layerSegments = new List<IImageSegment>();
    private Rectangle layerBounds;
    private int layerPaletteIdx;

    public Sprite2Tiff(Sprite2TiffParams parameters)
    {
        this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    public BinaryFormat Convert(ISprite source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var memoryStream = new MemoryStream();
        Tiff tiff = Tiff.ClientOpen("sprite", "w", memoryStream, new TiffStream());

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

        int i = 0;
        foreach (var layer in segmentGroups) {
            var layerSprite = new Sprite {
                Width = layer.Bounds.Width,
                Height = layer.Bounds.Height,
                Segments = new Collection<IImageSegment>(layer.Segments),
            };

            var layerIndexedImage = sprite2Image.Convert(layerSprite);

            if (parameters.ExportAsIndexedImage) {
                WritePage(
                    tiff,
                    layerIndexedImage,
                    layer.Bounds.Left + relativeX,
                    layer.Bounds.Top + relativeY,
                    i++,
                    segmentGroups.Count + 1);
            } else {
                var layerImage = layerIndexedImage.CreateFullImage(parameters.Palettes, true);
                WritePage(
                    tiff,
                    layerImage,
                    layer.Bounds.Left + relativeX,
                    layer.Bounds.Top + relativeY,
                    i++,
                    segmentGroups.Count + 1);
            }
        }

        WriteBgPage(tiff, parameters.ExportAsIndexedImage, source.Width, source.Height, segmentGroups.Count + 1);

        _ = tiff.Flush();

        var binary = new BinaryFormat(memoryStream);
        return binary;
    }

    private static ushort MapTo16Bits(uint num)
    {
        uint maxRangeCurrent = byte.MaxValue;
        uint maxRangeNext = ushort.MaxValue;

        num &= maxRangeCurrent;
        double result = (num * maxRangeNext) / (double)maxRangeCurrent;
        return (ushort)Math.Round(result);
    }

    private static void Assert(bool success)
    {
        if (!success) {
            throw new FormatException("Failed to write TIFF");
        }
    }

    private static Rectangle CreateRectangle(IImageSegment segment) =>
        new Rectangle(segment.CoordinateX, segment.CoordinateY, segment.Width, segment.Height);

    private bool AddSegmentIfSameLayer(IImageSegment segment)
    {
        if (layerPaletteIdx != segment.PaletteIndex) {
            return false;
        }

        var segmentBounds = new Rectangle(segment.CoordinateX, segment.CoordinateY, segment.Width, segment.Height);
        if (IntersectsWithLayer(segment)) {
            return false;
        }

        if (!IsAdjacentWithLayer(segment)) {
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

    private bool IntersectsWithLayer(IImageSegment segment)
    {
        var segmentBounds = CreateRectangle(segment);
        return layerSegments.Exists(s => CreateRectangle(s).IntersectsWith(segmentBounds));
    }

    private bool IsAdjacentWithLayer(IImageSegment segment)
    {
        static bool IsAdjacent(Rectangle rect1, Rectangle rect2) =>
            (rect1.Left == rect2.Right)
            || (rect1.Top == rect2.Bottom)
            || (rect1.Right == rect2.Left)
            || (rect1.Bottom == rect2.Top);

        var segmentBounds = CreateRectangle(segment);
        return layerSegments.Exists(s => IsAdjacent(CreateRectangle(s), segmentBounds));
    }

    private void WriteColorMap(Tiff tiff)
    {
        var fullPalette = parameters.Palettes.Palettes
            .SelectMany(p => p.Colors);

        // The colormap size must mach 1 << bitsPerSample
        int missingColors = 256 - fullPalette.Count();
        Rgb[] tiffColorMap = fullPalette.Concat(new Rgb[missingColors]).ToArray();

        Assert(tiff.SetField(
            TiffTag.COLORMAP,
            tiffColorMap.Select(c => MapTo16Bits(c.Red)).ToArray(),
            tiffColorMap.Select(c => MapTo16Bits(c.Green)).ToArray(),
            tiffColorMap.Select(c => MapTo16Bits(c.Blue)).ToArray()));
    }

    private void WritePage(Tiff tiff, IndexedImage image, int x, int y, int pageIdx, int totalPages)
    {
        int samplesPerPixel = 2; // palette index + alpha
        int bitsPerSample = 8; // 8-bits per palette idx

        Assert(tiff.SetField(TiffTag.IMAGEWIDTH, image.Width));
        Assert(tiff.SetField(TiffTag.IMAGELENGTH, image.Height));
        Assert(tiff.SetField(TiffTag.SAMPLESPERPIXEL, samplesPerPixel));
        Assert(tiff.SetField(TiffTag.BITSPERSAMPLE, bitsPerSample));
        Assert(tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT));
        Assert(tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG));
        Assert(tiff.SetField(TiffTag.ROWSPERSTRIP, tiff.DefaultStripSize(0)));
        Assert(tiff.SetField(TiffTag.XRESOLUTION, 100.0));
        Assert(tiff.SetField(TiffTag.YRESOLUTION, 100.0));
        Assert(tiff.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH));
        Assert(tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.PALETTE));
        Assert(tiff.SetField(TiffTag.EXTRASAMPLES, 1, new short[] { (short)ExtraSample.UNASSALPHA }));

        Assert(tiff.SetField(TiffTag.XPOSITION, x / 100.0));
        Assert(tiff.SetField(TiffTag.YPOSITION, y / 100.0));
        Assert(tiff.SetField(TiffTag.SUBFILETYPE, FileType.PAGE));
        Assert(tiff.SetField(TiffTag.PAGENUMBER, pageIdx, totalPages));
        Assert(tiff.SetField(TiffTag.PAGENAME, Encoding.ASCII.GetBytes($"Layer {pageIdx}")));

        // needs to be in each page but it must be the same between pages
        // so this method will write all palettes as a single one always.
        WriteColorMap(tiff);

        byte[] buffer = new byte[samplesPerPixel * image.Width];
        for (int h = 0; h < image.Height; h++) {
            for (int w = 0; w < image.Width; w++) {
                int idx = w + (h * image.Width);

                IndexedPixel pixel = image.Pixels[idx];
                int indexInBigPalette = pixel.Index + (pixel.PaletteIndex * 16);
                byte alpha = (pixel.Index == 0) ? (byte)0 : pixel.Alpha;

                buffer[w * samplesPerPixel] = (byte)indexInBigPalette;
                buffer[(w * samplesPerPixel) + 1] = alpha;
            }

            Assert(tiff.WriteScanline(buffer, h));
        }

        Assert(tiff.WriteDirectory());
    }

    private void WritePage(Tiff tiff, FullImage image, int x, int y, int pageIdx, int totalPages)
    {
        int samplesPerPixel = 4; // rgb + alpha
        int bitsPerSample = 8; // 8-bits per color

        Assert(tiff.SetField(TiffTag.IMAGEWIDTH, image.Width));
        Assert(tiff.SetField(TiffTag.IMAGELENGTH, image.Height));
        Assert(tiff.SetField(TiffTag.SAMPLESPERPIXEL, samplesPerPixel));
        Assert(tiff.SetField(TiffTag.BITSPERSAMPLE, bitsPerSample));
        Assert(tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT));
        Assert(tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG));
        Assert(tiff.SetField(TiffTag.ROWSPERSTRIP, tiff.DefaultStripSize(0)));
        Assert(tiff.SetField(TiffTag.XRESOLUTION, 100.0));
        Assert(tiff.SetField(TiffTag.YRESOLUTION, 100.0));
        Assert(tiff.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH));
        Assert(tiff.SetField(TiffTag.EXTRASAMPLES, 1, new short[] { (short)ExtraSample.UNASSALPHA }));
        Assert(tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB));

        Assert(tiff.SetField(TiffTag.XPOSITION, x / 100.0));
        Assert(tiff.SetField(TiffTag.YPOSITION, y / 100.0));
        Assert(tiff.SetField(TiffTag.SUBFILETYPE, FileType.PAGE));
        Assert(tiff.SetField(TiffTag.PAGENUMBER, pageIdx, totalPages));
        Assert(tiff.SetField(TiffTag.PAGENAME, Encoding.ASCII.GetBytes($"Segment {pageIdx}")));

        byte[] buffer = new byte[samplesPerPixel * image.Width];
        for (int h = 0; h < image.Height; h++) {
            for (int w = 0; w < image.Width; w++) {
                int idx = w + (h * image.Width);
                buffer[w * samplesPerPixel] = image.Pixels[idx].Red;
                buffer[(w * samplesPerPixel) + 1] = image.Pixels[idx].Green;
                buffer[(w * samplesPerPixel) + 2] = image.Pixels[idx].Blue;
                buffer[(w * samplesPerPixel) + 3] = image.Pixels[idx].Alpha;
            }

            Assert(tiff.WriteScanline(buffer, h));
        }

        Assert(tiff.WriteDirectory());
    }

    private void WriteBgPage(Tiff tiff, bool isIndexed, int width, int height, int totalPages)
    {
        // TODO: It doesn't work if indexed
        int samplesPerPixel = isIndexed ? 2 : 4; // rgb + alpha
        int bitsPerSample = 8; // 8-bits per color

        Assert(tiff.SetField(TiffTag.IMAGEWIDTH, width));
        Assert(tiff.SetField(TiffTag.IMAGELENGTH, height));
        Assert(tiff.SetField(TiffTag.SAMPLESPERPIXEL, samplesPerPixel));
        Assert(tiff.SetField(TiffTag.BITSPERSAMPLE, bitsPerSample));
        Assert(tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT));
        Assert(tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG));
        Assert(tiff.SetField(TiffTag.ROWSPERSTRIP, tiff.DefaultStripSize(0)));
        Assert(tiff.SetField(TiffTag.XRESOLUTION, 100.0));
        Assert(tiff.SetField(TiffTag.YRESOLUTION, 100.0));
        Assert(tiff.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH));
        Assert(tiff.SetField(TiffTag.EXTRASAMPLES, 1, new short[] { (short)ExtraSample.UNASSALPHA }));

        Assert(tiff.SetField(TiffTag.XPOSITION, 0 / 100.0));
        Assert(tiff.SetField(TiffTag.YPOSITION, 0 / 100.0));
        Assert(tiff.SetField(TiffTag.SUBFILETYPE, FileType.PAGE));
        Assert(tiff.SetField(TiffTag.PAGENUMBER, totalPages - 1, totalPages));
        Assert(tiff.SetField(TiffTag.PAGENAME, Encoding.ASCII.GetBytes($"Background - IGNORED")));

        if (isIndexed) {
            Assert(tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.PALETTE));
            WriteColorMap(tiff);
        } else {
            Assert(tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB));
        }

        byte[] buffer = new byte[samplesPerPixel * width];
        for (int h = 0; h < height; h++) {
            Assert(tiff.WriteScanline(buffer, h));
        }

        Assert(tiff.WriteDirectory());
    }
}
