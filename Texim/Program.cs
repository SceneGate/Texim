//
// Program.cs
//
// Author:
//       Benito Palacios Sanchez <benito356@gmail.com>
//
// Copyright (c) 2017 Benito Palacios Sanchez
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace Texim
{
    using System.Drawing;
    using System.Linq;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;

    class MainClass
    {
        public static void Main(string[] args)
        {
            DevilSurvivor(args);
        }

        static void DevilSurvivor(string[] args)
        {
            string palettePath = args[0];
            string newImagePath = args[1];
            string outPath = args[2];

            // Load palette to force colors when importing
            Node palette = NodeFactory.FromFile(palettePath);
            palette.Transform<BinaryFormat, Palette, DsTex2Palette>();

            // The palette may have more colors than needed.
            // Since the format is A3I5 only 32 colors are used
            Color[] actualPalette = palette.GetFormatAs<Palette>().GetPalette(0)
                   .Take(32)
                   .ToArray();

            // To see the image:
            //Node pixels = NodeFactory.FromFile("auction_price.cmp.decompressed");
            //pixels.Transform<BinaryFormat, Image, DsTex2Image>();
            //pixels.GetFormatAs<Image>()
            //    .CreateBitmap(palette.GetFormatAs<Palette>(), 0).Save("img.png");

            // Import the new PNG file
            Bitmap newImage = (Bitmap)System.Drawing.Image.FromFile(newImagePath);
            var quantization = new FixedPaletteQuantization(actualPalette) {
                TransparentIndex = 0x15
            };
            Importer importer = new Importer {
                Format = ColorFormat.Indexed_A3I5,
                PixelEncoding = PixelEncoding.Lineal,
                Quantization = quantization
            };
            importer.Import(newImage, out Palette _, out Image newTiles);

            // Save the new pixel info
            Node newPixels = new Node("pxInfo", newTiles);
            newPixels.Transform<Image, BinaryFormat, DsTex2Image>();
            newPixels.GetFormatAs<BinaryFormat>().Stream.WriteTo(outPath);
        }
    }
}
