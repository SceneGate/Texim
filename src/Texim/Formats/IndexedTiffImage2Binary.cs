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

public class IndexedTiffImage2Binary : IConverter<TiffImage, BinaryFormat>
{
    private const int SamplesPerPixel = 2; // palette index + alpha
    private const int BitsPerSample = 8; // 8-bits per palette idx (max 256 colors in palette)

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
            WritePage(tiff, page, outputPages);
        }

        WriteBgPage(tiff, source.MainImage.Width, source.MainImage.Height, outputPages);

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

    private void WriteColorMap(Tiff tiff, Rgb[] colorMap)
    {
        // The colormap size must mach 1 << bitsPerSample
        int requiredColors = 1 << BitsPerSample;
        int missingColors = requiredColors - colorMap.Length;
        Rgb[] tiffColorMap = colorMap.Concat(new Rgb[missingColors]).ToArray();

        Assert(tiff.SetField(
            TiffTag.COLORMAP,
            tiffColorMap.Select(c => MapTo16Bits(c.Red)).ToArray(),
            tiffColorMap.Select(c => MapTo16Bits(c.Green)).ToArray(),
            tiffColorMap.Select(c => MapTo16Bits(c.Blue)).ToArray()));
    }

    private void WritePage(Tiff tiff, TiffPage page, int totalPages)
    {
        Assert(tiff.SetField(TiffTag.IMAGEWIDTH, page.Width));
        Assert(tiff.SetField(TiffTag.IMAGELENGTH, page.Height));
        Assert(tiff.SetField(TiffTag.SAMPLESPERPIXEL, SamplesPerPixel));
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

        byte[] buffer = new byte[SamplesPerPixel * page.Width];
        for (int h = 0; h < page.Height; h++) {
            for (int w = 0; w < page.Width; w++) {
                int idx = w + (h * page.Width);

                IndexedPixel pixel = page.IndexedPixels[idx];
                int indexInBigPalette = pixel.Index + (pixel.PaletteIndex * 16);
                byte alpha = (pixel.Index == 0) ? (byte)0 : pixel.Alpha;

                buffer[w * SamplesPerPixel] = (byte)indexInBigPalette;
                buffer[(w * SamplesPerPixel) + 1] = alpha;
            }

            Assert(tiff.WriteScanline(buffer, h));
        }

        Assert(tiff.WriteDirectory());
    }

    private void WriteBgPage(Tiff tiff, int width, int height, int totalPages)
    {
        Assert(tiff.SetField(TiffTag.IMAGEWIDTH, width));
        Assert(tiff.SetField(TiffTag.IMAGELENGTH, height));
        Assert(tiff.SetField(TiffTag.SAMPLESPERPIXEL, SamplesPerPixel));
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

        Assert(tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.PALETTE));
        WriteColorMap(tiff, new Rgb[] { new Rgb(0, 0, 0) });

        byte[] buffer = new byte[SamplesPerPixel * width];
        for (int h = 0; h < height; h++) {
            Assert(tiff.WriteScanline(buffer, h));
        }

        Assert(tiff.WriteDirectory());
    }
}
