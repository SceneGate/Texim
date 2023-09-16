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
using System.Text;
using BitMiracle.LibTiff.Classic;
using Texim.Colors;
using Texim.Pixels;
using Yarhl.FileFormat;
using Yarhl.IO;

public class Tiff2Binary : IConverter<TiffImage, BinaryFormat>
{
    private const int IndexedSamplesPerPixel = 2; // palette index + alpha
    private const int RgbaSamplesPerPixel = 4; // red, green, blue and alpha

    // 8-bits per palette idx (max 256 colors in palette)
    // or 256 values per component max.
    private const int BitsPerSample = 8;

    public BinaryFormat Convert(TiffImage source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!source.MainImage?.IsIndexed ?? false) {
            throw new FormatException("Invalid pixel format");
        }

        var memoryStream = new MemoryStream();
        var tiff = Tiff.ClientOpen("output", "w", memoryStream, new TiffStream());

        int outputPages = source.Pages.Count + 1;
        foreach (TiffPage page in source.Pages) {
            if (page.IsIndexed) {
                WriteIndexedPage(tiff, page, outputPages);
            } else {
                WriteRgbPage(tiff, page, outputPages);
            }
        }

        bool isIndexed = source.Pages.Any(p => p.IsIndexed);
        WriteBgPage(tiff, isIndexed, source.MainImage.ColorMap, source.MainImage.Width, source.MainImage.Height, outputPages);

        _ = tiff.Flush();

        return new BinaryFormat(memoryStream);
    }

    private static ushort MapTo16Bits(uint num)
    {
        const uint maxRangeCurrent = byte.MaxValue;
        const uint maxRangeNext = ushort.MaxValue;

        num &= maxRangeCurrent;
        double result = num * maxRangeNext / (double)maxRangeCurrent;
        return (ushort)Math.Round(result);
    }

    private static void Assert(bool success)
    {
        if (!success) {
            throw new FormatException("Failed to write TIFF");
        }
    }

    private static void WriteColorMap(Tiff tiff, Rgb[] colorMap)
    {
        // The colormap size must mach 1 << bitsPerSample
        const int requiredColors = 1 << BitsPerSample;
        int missingColors = requiredColors - colorMap.Length;
        Rgb[] tiffColorMap = colorMap.Concat(new Rgb[missingColors]).ToArray();

        Assert(tiff.SetField(
            TiffTag.COLORMAP,
            tiffColorMap.Select(c => MapTo16Bits(c.Red)).ToArray(),
            tiffColorMap.Select(c => MapTo16Bits(c.Green)).ToArray(),
            tiffColorMap.Select(c => MapTo16Bits(c.Blue)).ToArray()));
    }

    private static void WriteIndexedPage(Tiff tiff, TiffPage page, int totalPages)
    {
        Assert(tiff.SetField(TiffTag.IMAGEWIDTH, page.Width));
        Assert(tiff.SetField(TiffTag.IMAGELENGTH, page.Height));
        Assert(tiff.SetField(TiffTag.SAMPLESPERPIXEL, IndexedSamplesPerPixel));
        Assert(tiff.SetField(TiffTag.BITSPERSAMPLE, BitsPerSample));
        Assert(tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT));
        Assert(tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG));
        Assert(tiff.SetField(TiffTag.ROWSPERSTRIP, tiff.DefaultStripSize(0)));
        Assert(tiff.SetField(TiffTag.XRESOLUTION, 100.0));
        Assert(tiff.SetField(TiffTag.YRESOLUTION, 100.0));
        Assert(tiff.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH));
        Assert(tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.PALETTE));
        Assert(tiff.SetField(TiffTag.EXTRASAMPLES, 1, (short)ExtraSample.UNASSALPHA));

        Assert(tiff.SetField(TiffTag.XPOSITION, page.X / 100.0));
        Assert(tiff.SetField(TiffTag.YPOSITION, page.Y / 100.0));
        Assert(tiff.SetField(TiffTag.SUBFILETYPE, FileType.PAGE));
        Assert(tiff.SetField(TiffTag.PAGENUMBER, page.Id, totalPages));
        Assert(tiff.SetField(TiffTag.PAGENAME, Encoding.ASCII.GetBytes($"Layer {page.Id}")));

        // needs to be in each page but it must be the same between pages
        // so this method will write all palettes as a single one always.
        WriteColorMap(tiff, page.ColorMap);

        byte[] buffer = new byte[IndexedSamplesPerPixel * page.Width];
        for (int h = 0; h < page.Height; h++) {
            for (int w = 0; w < page.Width; w++) {
                int idx = w + (h * page.Width);

                IndexedPixel pixel = page.IndexedPixels[idx];
                int indexInBigPalette = pixel.Index + (pixel.PaletteIndex * 16);
                byte alpha = (pixel.Index == 0) ? (byte)0 : pixel.Alpha;

                buffer[w * IndexedSamplesPerPixel] = (byte)indexInBigPalette;
                buffer[(w * IndexedSamplesPerPixel) + 1] = alpha;
            }

            Assert(tiff.WriteScanline(buffer, h));
        }

        Assert(tiff.WriteDirectory());
    }

    private static void WriteRgbPage(Tiff tiff, TiffPage page, int totalPages)
    {
        Assert(tiff.SetField(TiffTag.IMAGEWIDTH, page.Width));
        Assert(tiff.SetField(TiffTag.IMAGELENGTH, page.Height));
        Assert(tiff.SetField(TiffTag.SAMPLESPERPIXEL, RgbaSamplesPerPixel));
        Assert(tiff.SetField(TiffTag.BITSPERSAMPLE, BitsPerSample));
        Assert(tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT));
        Assert(tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG));
        Assert(tiff.SetField(TiffTag.ROWSPERSTRIP, tiff.DefaultStripSize(0)));
        Assert(tiff.SetField(TiffTag.XRESOLUTION, 100.0));
        Assert(tiff.SetField(TiffTag.YRESOLUTION, 100.0));
        Assert(tiff.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH));
        Assert(tiff.SetField(TiffTag.EXTRASAMPLES, 1, (short)ExtraSample.UNASSALPHA));
        Assert(tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB));

        Assert(tiff.SetField(TiffTag.XPOSITION, page.X / 100.0));
        Assert(tiff.SetField(TiffTag.YPOSITION, page.Y / 100.0));
        Assert(tiff.SetField(TiffTag.SUBFILETYPE, FileType.PAGE));
        Assert(tiff.SetField(TiffTag.PAGENUMBER, page.Id, totalPages));
        Assert(tiff.SetField(TiffTag.PAGENAME, Encoding.ASCII.GetBytes($"Segment {page.Id}")));

        byte[] buffer = new byte[RgbaSamplesPerPixel * page.Width];
        for (int h = 0; h < page.Height; h++) {
            for (int w = 0; w < page.Width; w++) {
                int idx = w + (h * page.Width);
                buffer[w * RgbaSamplesPerPixel] = page.RgbPixels[idx].Red;
                buffer[(w * RgbaSamplesPerPixel) + 1] = page.RgbPixels[idx].Green;
                buffer[(w * RgbaSamplesPerPixel) + 2] = page.RgbPixels[idx].Blue;
                buffer[(w * RgbaSamplesPerPixel) + 3] = page.RgbPixels[idx].Alpha;
            }

            Assert(tiff.WriteScanline(buffer, h));
        }

        Assert(tiff.WriteDirectory());
    }

    private static void WriteBgPage(Tiff tiff, bool isIndexed, Rgb[] palette, int width, int height, int totalPages)
    {
        int samplesPerPixel = isIndexed ? IndexedSamplesPerPixel : RgbaSamplesPerPixel;

        Assert(tiff.SetField(TiffTag.IMAGEWIDTH, width));
        Assert(tiff.SetField(TiffTag.IMAGELENGTH, height));
        Assert(tiff.SetField(TiffTag.SAMPLESPERPIXEL, samplesPerPixel));
        Assert(tiff.SetField(TiffTag.BITSPERSAMPLE, BitsPerSample));
        Assert(tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT));
        Assert(tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG));
        Assert(tiff.SetField(TiffTag.ROWSPERSTRIP, tiff.DefaultStripSize(0)));
        Assert(tiff.SetField(TiffTag.XRESOLUTION, 100.0));
        Assert(tiff.SetField(TiffTag.YRESOLUTION, 100.0));
        Assert(tiff.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH));
        Assert(tiff.SetField(TiffTag.EXTRASAMPLES, 1, (short)ExtraSample.UNASSALPHA));

        Assert(tiff.SetField(TiffTag.XPOSITION, 0 / 100.0));
        Assert(tiff.SetField(TiffTag.YPOSITION, 0 / 100.0));
        Assert(tiff.SetField(TiffTag.SUBFILETYPE, FileType.PAGE));
        Assert(tiff.SetField(TiffTag.PAGENUMBER, totalPages - 1, totalPages));
        Assert(tiff.SetField(TiffTag.PAGENAME, Encoding.ASCII.GetBytes("Background - IGNORED")));

        if (isIndexed) {
            Assert(tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.PALETTE));
            WriteColorMap(tiff, palette);
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
