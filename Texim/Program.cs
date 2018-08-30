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
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using BlackRockShooter;
    using DevilSurvivor;
    using MetalMax;
    using Media.Image;
    using Media.Image.Processing;

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Texim -- Experimental library for game images");
            Console.WriteLine("v{0} ~~ by pleonex ~~", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine();

            string format;
            if (args.Length < 1) {
                Console.Write("Game (devilsurvivor|metalmax|brs): ");
                format = Console.ReadLine();
            } else {
                format = args[0];
                args = args.Skip(1).ToArray();
            }

            if (format == "devilsurvivor")
                DevilSurvivor(args);
            else if (format == "metalmax")
                MetalMax(args);
            else if (format == "brs")
                Brs(args);
            else
                Console.WriteLine("Game is not supported");

            Console.WriteLine("Done!");
        }

        static void Brs(string[] args)
        {
            string binaryFile;
            string imageFile;

            if (args.Length == 2) {
                binaryFile = args[0];
                imageFile = args[1];
            } else {
                Console.Write("Input file: ");
                binaryFile = Console.ReadLine();

                Console.Write("Output file: ");
                imageFile = Console.ReadLine();
            }

            using (var binaryFormat = new BinaryFormat(binaryFile)) {
                Ptp ptp = binaryFormat.ConvertWith<Binary2Ptp, BinaryFormat, Ptp>();
                ptp.Pixels.CreateBitmap(ptp.Palette, 0).Save(imageFile);
            }
        }

        static void MetalMax(string[] args)
        {
            string oldImagePath;
            string newImagePath;
            string outPath;

            if (args.Length == 3) {
                oldImagePath = args[0];
                newImagePath = args[1];
                outPath = args[2];
            } else {
                Console.Write("Input file: ");
                oldImagePath = Console.ReadLine();

                Console.Write("Image to import: ");
                newImagePath = Console.ReadLine();

                Console.Write("Output file: ");
                outPath = Console.ReadLine();
            }

            // Load palette to force colors when importing
            BinaryFormat currentTex = new BinaryFormat(oldImagePath);
            MmTex texture = currentTex.ConvertWith<Binary2MmTex, BinaryFormat, MmTex>();
            currentTex.Dispose();

            // To export the image:
            //oldPixels.CreateBitmap(palette, 0).Save("img.png");

            // Import the new PNG file
            Bitmap newImage = (Bitmap)Image.FromFile(newImagePath);
            var quantization = new FixedPaletteQuantization(texture.Palette.GetPalette(0));
            Media.Image.ImageConverter importer = new Media.Image.ImageConverter {
                Format = ColorFormat.Indexed_A5I3,
                PixelEncoding = PixelEncoding.Lineal,
                Quantization = quantization
            };
            (Palette _, PixelArray pixelInfo) = importer.Convert(newImage);
            texture.Pixels = pixelInfo;

            // Save the texture
            texture.ConvertWith<Binary2MmTex, MmTex, BinaryFormat>()
                  .Stream.WriteTo(outPath);
        }

        static void DevilSurvivor(string[] args)
        {
            string palettePath;
            string newImagePath;
            string outPath;

            if (args.Length == 3) {
                palettePath = args[0];
                newImagePath = args[1];
                outPath = args[2];
            } else {
                Console.Write("Palette file: ");
                palettePath = Console.ReadLine();

                Console.Write("Image to import: ");
                newImagePath = Console.ReadLine();

                Console.Write("Output file: ");
                outPath = Console.ReadLine();
            }

            // Load palette to force colors when importing
            Node palette = NodeFactory.FromFile(palettePath);
            palette.Transform<DsTex2Palette, BinaryFormat, Palette>();

            // The palette may have more colors than needed.
            // Since the format is A3I5 only 32 colors are used
            Color[] actualPalette = palette.GetFormatAs<Palette>().GetPalette(0)
                   .Take(32)
                   .ToArray();

            // To export the image:
            //Node pixels = NodeFactory.FromFile("auction_price.cmp.decompressed");
            //pixels.Transform<BinaryFormat, PixelArray, DsTex2Image>();
            //pixels.GetFormatAs<PixelArray>()
            //    .CreateBitmap(palette.GetFormatAs<Palette>(), 0).Save("img.png");

            // Import the new PNG file
            Bitmap newImage = (Bitmap)Image.FromFile(newImagePath);
            var quantization = new FixedPaletteQuantization(actualPalette) {
                TransparentIndex = 0x15
            };
            Media.Image.ImageConverter importer = new Media.Image.ImageConverter {
                Format = ColorFormat.Indexed_A3I5,
                PixelEncoding = PixelEncoding.Lineal,
                Quantization = quantization
            };
            (Palette _, PixelArray pixelInfo) = importer.Convert(newImage);

            // Save the new pixel info
            Node newPixels = new Node("pxInfo", pixelInfo);
            newPixels.Transform<DsTex2Image, PixelArray, BinaryFormat>();
            newPixels.GetFormatAs<BinaryFormat>().Stream.WriteTo(outPath);
        }
    }
}
