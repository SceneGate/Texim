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
using System.Text;
using BitMiracle.LibTiff.Classic;
using Texim.Colors;
using Texim.Pixels;
using Yarhl.FileFormat;
using Yarhl.IO;

public class Binary2Tiff : IConverter<IBinary, TiffImage>
{
    private const string BackgroundName = "Background - IGNORED";

    public TiffImage Convert(IBinary source)
    {
        ArgumentNullException.ThrowIfNull(source);

        using var tiff = Tiff.ClientOpen("sprite", "r", source.Stream, new TiffStream());
        var image = new TiffImage();

        do {
            byte[] encodedName = tiff.GetFieldDefaulted(TiffTag.PAGENAME)?[0].GetBytes();
            if (encodedName is not null && Encoding.ASCII.GetString(encodedName) == BackgroundName) {
                continue;
            }

            object pixelsType = tiff.GetField(TiffTag.PHOTOMETRIC)[0].Value;
            if (pixelsType is Photometric.PALETTE) {
                image.Pages.Add(ReadIndexedPage(tiff));
            } else if (pixelsType is Photometric.RGB) {
                image.Pages.Add(ReadRgbPage(tiff));
            } else {
                throw new FormatException($"Pixel type not supported: {pixelsType}");
            }
        } while (tiff.ReadDirectory());

        return image;
    }

    private static FieldValue GetFieldSingle(Tiff tiff, TiffTag tag)
    {
        var values = tiff.GetField(tag);
        if (values is null) {
            throw new FormatException($"Expecting TIFF tag: {tag}");
        }

        if (values.Length == 0) {
            throw new FormatException($"Missing values for tag: {tag}");
        }

        if (values.Length != 1) {
            throw new FormatException($"Got more than one value for tag: {tag}");
        }

        return values[0];
    }

    private static TiffPage ReadRgbPage(Tiff tiff)
    {
        var page = new TiffPage();

        page.Width = GetFieldSingle(tiff, TiffTag.IMAGEWIDTH).ToInt();
        page.Height = GetFieldSingle(tiff, TiffTag.IMAGELENGTH).ToInt();

        int samplesPerPixel = GetFieldSingle(tiff, TiffTag.SAMPLESPERPIXEL).ToInt();
        if (samplesPerPixel != 4) {
            throw new NotSupportedException($"Invalid samples per pixel: {samplesPerPixel}");
        }

        int bitsPerSample = GetFieldSingle(tiff, TiffTag.BITSPERSAMPLE).ToInt();
        if (bitsPerSample != 8) {
            throw new NotSupportedException($"Invalid bits per sample: {bitsPerSample}");
        }

        object extraSamples = GetFieldSingle(tiff, TiffTag.EXTRASAMPLES).Value;
        if (extraSamples is not ExtraSample.UNASSALPHA) {
            throw new NotSupportedException($"Invalid extra samples: {extraSamples}");
        }

        object orientation = GetFieldSingle(tiff, TiffTag.ORIENTATION).Value;
        if (orientation is not Orientation.TOPLEFT) {
            throw new NotSupportedException($"Orientation not supported: {orientation}");
        }

        object planar = GetFieldSingle(tiff, TiffTag.PLANARCONFIG).Value;
        if (planar is not PlanarConfig.CONTIG) {
            throw new NotSupportedException($"Planar not supported: {planar}");
        }

        int xRes = GetFieldSingle(tiff, TiffTag.XRESOLUTION).ToInt();
        int xPos = GetFieldSingle(tiff, TiffTag.XPOSITION).ToInt();
        page.X = xPos * xRes;

        int yRes = GetFieldSingle(tiff, TiffTag.YRESOLUTION).ToInt();
        int yPos = GetFieldSingle(tiff, TiffTag.YPOSITION).ToInt();
        page.Y = yPos * yRes;

        page.IsIndexed = false;
        page.RgbPixels = new Rgb[page.Width * page.Height];
        byte[] row = new byte[samplesPerPixel * page.Width];
        for (int y = 0; y < page.Height; y++) {
            _ = tiff.ReadScanline(row, y);

            for (int x = 0; x < page.Width; x++) {
                int index = (y * page.Width) + x;
                var color = new Rgb(
                    row[x * samplesPerPixel],
                    row[(x * samplesPerPixel) + 1],
                    row[(x * samplesPerPixel) + 2],
                    row[(x * samplesPerPixel) + 3]);
                page.RgbPixels[index] = color;
            }
        }

        return page;
    }

    private static TiffPage ReadIndexedPage(Tiff tiff)
    {
        var page = new TiffPage();

        page.Width = GetFieldSingle(tiff, TiffTag.IMAGEWIDTH).ToInt();
        page.Height = GetFieldSingle(tiff, TiffTag.IMAGELENGTH).ToInt();

        int samplesPerPixel = GetFieldSingle(tiff, TiffTag.SAMPLESPERPIXEL).ToInt();
        if (samplesPerPixel != 2) {
            throw new NotSupportedException($"Invalid samples per pixel: {samplesPerPixel}");
        }

        int bitsPerSample = GetFieldSingle(tiff, TiffTag.BITSPERSAMPLE).ToInt();
        if (bitsPerSample != 8) {
            throw new NotSupportedException($"Invalid bits per sample: {bitsPerSample}");
        }

        object extraSamples = GetFieldSingle(tiff, TiffTag.EXTRASAMPLES).Value;
        if (extraSamples is not ExtraSample.UNASSALPHA) {
            throw new NotSupportedException($"Invalid extra samples: {extraSamples}");
        }

        object orientation = GetFieldSingle(tiff, TiffTag.ORIENTATION).Value;
        if (orientation is not Orientation.TOPLEFT) {
            throw new NotSupportedException($"Orientation not supported: {orientation}");
        }

        object planar = GetFieldSingle(tiff, TiffTag.PLANARCONFIG).Value;
        if (planar is not PlanarConfig.CONTIG) {
            throw new NotSupportedException($"Planar not supported: {planar}");
        }

        int xRes = GetFieldSingle(tiff, TiffTag.XRESOLUTION).ToInt();
        int xPos = GetFieldSingle(tiff, TiffTag.XPOSITION).ToInt();
        page.X = xPos * xRes;

        int yRes = GetFieldSingle(tiff, TiffTag.YRESOLUTION).ToInt();
        int yPos = GetFieldSingle(tiff, TiffTag.YPOSITION).ToInt();
        page.Y = yPos * yRes;

        page.IsIndexed = true;
        page.IndexedPixels = new IndexedPixel[page.Width * page.Height];
        byte[] row = new byte[samplesPerPixel * page.Width];
        for (int y = 0; y < page.Height; y++) {
            _ = tiff.ReadScanline(row, y);

            for (int x = 0; x < page.Width; x++) {
                int index = (y * page.Width) + x;
                var pixel = new IndexedPixel(
                    row[x * samplesPerPixel],
                    row[(x * samplesPerPixel) + 1]);
                page.IndexedPixels[index] = pixel;
            }
        }

        FieldValue[] colorMapFields = tiff.GetField(TiffTag.COLORMAP);
        if (colorMapFields is null || colorMapFields.Length != 3) {
            throw new FormatException("Invalid color map");
        }

        short[] reds = colorMapFields[0].ToShortArray();
        short[] blues = colorMapFields[0].ToShortArray();
        short[] greens = colorMapFields[0].ToShortArray();
        page.ColorMap = new Rgb[reds.Length];
        for (int i = 0; i < reds.Length; i++) {
            page.ColorMap[i] = new Rgb(
                MapTo8Bits(reds[i]),
                MapTo8Bits(blues[i]),
                MapTo8Bits(greens[i]));
        }

        return page;
    }

    private static byte MapTo8Bits(int num)
    {
        const int maxRangeCurrent = short.MaxValue;
        const uint maxRangeNext = byte.MaxValue;

        num &= maxRangeCurrent;
        double result = num * maxRangeNext / (double)maxRangeCurrent;
        return (byte)Math.Round(result);
    }
}
