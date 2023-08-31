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
using System.IO;
using System.Linq;
using BitMiracle.LibTiff.Classic;
using Texim.Images;
using Texim.Palettes;
using Texim.Sprites;
using Yarhl.FileFormat;
using Yarhl.IO;

public class Sprite2Tiff : IConverter<ISprite, BinaryFormat>
{
    private readonly IPaletteCollection palettes;
    private readonly ImageSegment2IndexedImageParams indexedParams;
    private readonly SpriteRelativeCoordinatesKind relativeCoords;

    public Sprite2Tiff(
        IPaletteCollection palettes,
        ImageSegment2IndexedImageParams indexedParams,
        SpriteRelativeCoordinatesKind relativeCoords)
    {
        this.palettes = palettes;
        this.indexedParams = indexedParams;
        this.relativeCoords = relativeCoords;
    }

    public BinaryFormat Convert(ISprite source)
    {
        ArgumentNullException.ThrowIfNull(source);

        ImageSegment2IndexedImage segmentConverter = new();
        segmentConverter.Initialize(indexedParams);

        var memoryStream = new MemoryStream();
        Tiff tiff = Tiff.ClientOpen("sprite", "w", memoryStream, new TiffStream());

        (int relativeX, int relativeY) = relativeCoords switch {
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
        for (int i = 0; i < orderedSegments.Length; i++) {
            var segment = orderedSegments[i];
            FullImage segmentImage = segmentConverter.Convert(segment)
                .CreateFullImage(palettes, true);

            WritePage(
                tiff,
                segmentImage,
                segment.CoordinateX + relativeX,
                segment.CoordinateY + relativeY,
                i,
                orderedSegments.Length);
        }

        tiff.Flush();

        var binary = new BinaryFormat(memoryStream);
        return binary;
    }

    private void WritePage(Tiff tiff, FullImage image, int x, int y, int id, int total)
    {
        int samplesPerPixel = 4; // rgb + alpha
        int bitsPerSample = 8; // 8-bits per color

        tiff.SetField(TiffTag.IMAGEWIDTH, image.Width);
        tiff.SetField(TiffTag.IMAGELENGTH, image.Height);
        tiff.SetField(TiffTag.SAMPLESPERPIXEL, samplesPerPixel);
        tiff.SetField(TiffTag.BITSPERSAMPLE, bitsPerSample);
        tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
        tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
        tiff.SetField(TiffTag.ROWSPERSTRIP, tiff.DefaultStripSize(0));
        tiff.SetField(TiffTag.XRESOLUTION, 100.0);
        tiff.SetField(TiffTag.YRESOLUTION, 100.0);
        tiff.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);
        tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
        tiff.SetField(TiffTag.EXTRASAMPLES, 1, new short[] { (short)ExtraSample.UNASSALPHA });

        tiff.SetField(TiffTag.XPOSITION, x / 100.0);
        tiff.SetField(TiffTag.YPOSITION, y / 100.0);
        tiff.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
        tiff.SetField(TiffTag.PAGENUMBER, id, total);

        byte[] buffer = new byte[samplesPerPixel * image.Width];
        for (int h = 0; h < image.Height; h++) {
            for (int w = 0; w < image.Width; w++) {
                int idx = w + (h * image.Width);
                buffer[w * samplesPerPixel] = image.Pixels[idx].Red;
                buffer[(w * samplesPerPixel) + 1] = image.Pixels[idx].Green;
                buffer[(w * samplesPerPixel) + 2] = image.Pixels[idx].Blue;
                buffer[(w * samplesPerPixel) + 3] = image.Pixels[idx].Alpha;
            }

            tiff.WriteScanline(buffer, h);
        }

        tiff.WriteDirectory();
    }
}
