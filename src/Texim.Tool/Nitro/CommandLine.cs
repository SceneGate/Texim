// Copyright (c) 2021 SceneGate

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
namespace Texim.Tool.Nitro
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Linq;
    using Texim.Compressions.Nitro;
    using Texim.Formats;
    using Texim.Images;
    using Texim.Palettes;
    using Texim.Processing;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    public static class CommandLine
    {
        public static Symbol CreateCommand()
        {
            var exportPalette = new Command("export_palette", "Export Nitro files into an image") {
                new Option<string>("--input", "nitro palette file", ArgumentArity.ExactlyOne),
                new Option<string>("--output", "Output file", ArgumentArity.ExactlyOne),
                new Option<StandardPaletteFormat>("format", "Output format", ArgumentArity.ZeroOrOne),
            };
            exportPalette.Handler = CommandHandler.Create<string, string, StandardPaletteFormat>(ExportPalette);

            var exportImage = new Command("export_image", "Export Nitro files into an image") {
                new Option<string>("--nclr", "nitro palette file", ArgumentArity.ExactlyOne),
                new Option<string>("--ncgr", "nitro pixel file", ArgumentArity.ExactlyOne),
                new Option<string>("--nscr", "nitro screen file", ArgumentArity.ZeroOrOne),
                new Option<string>("--output", "Output file", ArgumentArity.ExactlyOne),
                new Option<StandardImageFormat>("format", "Output image format", ArgumentArity.ZeroOrOne),
            };
            exportImage.Handler = CommandHandler.Create<string, string, string, string, StandardImageFormat>(ExportImage);

            var importImage = new Command("import_image", "Import an image as Nitro image") {
                new Option<string>("--input", "the input image file", ArgumentArity.ExactlyOne),
                new Option<string>("--nclr", "the palette to use", ArgumentArity.ExactlyOne),
                new Option<string>("--ncgr", "optional original NCGR to copy params", ArgumentArity.ZeroOrOne),
                new Option<int>("--palette-index", "optional palette index", ArgumentArity.ZeroOrOne),
                new Option<string>("--output", "the output nitro image file", ArgumentArity.ExactlyOne),
            };
            importImage.Handler = CommandHandler.Create<string, string, string, string, int>(ImportImage);

            var importCompressedImage = new Command("import_compressed", "Import and compress an image") {
                new Option<string>("--input", "the input image file", ArgumentArity.ExactlyOne),
                new Option<string>("--nclr", "the palette to use", ArgumentArity.ExactlyOne),
                new Option<string>("--ncgr", "optional original NCGR to copy params and tiles", ArgumentArity.ZeroOrOne),
                new Option<bool>("--append", "re-use existing tiles from the NCGR", ArgumentArity.ZeroOrOne),
                new Option<string>("--nscr", "optional original NSCR to copy params", ArgumentArity.ZeroOrOne),
                new Option<bool>("--use-same-palettes", "force to use the same palette range", ArgumentArity.ZeroOrOne),
                new Option<string>("--out-ncgr", "the output nitro image file", ArgumentArity.ExactlyOne),
                new Option<string>("--out-nscr", "the output nitro compression file", ArgumentArity.ExactlyOne),
            };
            importCompressedImage.Handler = CommandHandler.Create<string, string, string, bool, string, bool, string, string>(ImportCompressedImage);

            return new Command("nitro", "Nintendo DS standard formats") {
                exportPalette,
                exportImage,
                importImage,
                importCompressedImage,
            };
        }

        private static void ExportPalette(string input, string output, StandardPaletteFormat format)
        {
            Nclr nclr = NodeFactory.FromFile(input, FileOpenMode.Read)
                .TransformWith<Binary2Nclr>()
                .GetFormatAs<Nclr>();

            string extension;
            IConverter<IPalette, BinaryFormat> converter;
            if (format == StandardPaletteFormat.Png) {
                extension = ".png";
                converter = new Palette2Bitmap();
            } else if (format == StandardPaletteFormat.Riff) {
                extension = ".pal";
                converter = new Palette2BinaryRiff();
            } else {
                throw new FormatException("Unknown output format");
            }

            string name = Path.GetFileNameWithoutExtension(input);
            for (int i = 0; i < nclr.Palettes.Count; i++) {
                using BinaryFormat outputStream = converter.Convert(nclr.Palettes[i]);

                string outputPath = Path.Combine(output, $"{name}_{i}.{extension}");
                outputStream.Stream.WriteTo(outputPath);
            }
        }

        private static void ExportImage(
            string nclr,
            string ncgr,
            string nscr,
            string output,
            StandardImageFormat format)
        {
            var palette = NodeFactory.FromFile(nclr, FileOpenMode.Read)
                .TransformWith<Binary2Nclr>()
                .GetFormatAs<Nclr>();

            var pixels = NodeFactory.FromFile(ncgr, FileOpenMode.Read)
                .TransformWith<Binary2Ncgr>();

            if (!string.IsNullOrEmpty(nscr)) {
                var map = NodeFactory.FromFile(nscr, FileOpenMode.Read)
                    .TransformWith<Binary2Nscr>()
                    .GetFormatAs<Nscr>();

                var mapParams = new MapDecompressionParams { Map = map };
                pixels.TransformWith<MapDecompression, MapDecompressionParams>(mapParams);
            }

            var indexedParams = new IndexedImageBitmapParams {
                Palettes = palette,
                Format = format.GetFormat(),
            };
            pixels.TransformWith<IndexedImage2Bitmap, IndexedImageBitmapParams>(indexedParams)
                .Stream.WriteTo(output);
        }

        private static void ImportImage(string input, string nclr, string output, string ncgr, int paletteIndex = 0)
        {
            var palette = NodeFactory.FromFile(nclr, FileOpenMode.Read)
                .TransformWith<Binary2Nclr>()
                .GetFormatAs<Nclr>();

            var quantization = new FixedPaletteQuantization(palette.Palettes[paletteIndex]);
            var newPixels = NodeFactory.FromFile(input, FileOpenMode.Read)
                .TransformWith<Bitmap2FullImage>()
                .TransformWith<FullImage2IndexedPalette, IQuantization>(quantization)
                .GetFormatAs<IndexedImage>();

            Ncgr newNcgr;
            if (ncgr == null) {
                newNcgr = new Ncgr(newPixels);
            } else {
                var originalNcgr = NodeFactory.FromFile(ncgr, FileOpenMode.Read)
                    .TransformWith<Binary2Ncgr>()
                    .GetFormatAs<Ncgr>();
                newNcgr = new Ncgr(originalNcgr, newPixels);
            }

            using var binary = new Ncgr2Binary().Convert(newNcgr);
            binary.Stream.WriteTo(output);
        }

        private static void ImportCompressedImage(
            string input,
            string nclr,
            string ncgr,
            bool append,
            string nscr,
            bool useSamePalettes,
            string outNcgr,
            string outNscr)
        {
            IPaletteCollection palette = NodeFactory.FromFile(nclr, FileOpenMode.Read)
                .TransformWith<Binary2Nclr>()
                .GetFormatAs<Nclr>();

            if (useSamePalettes) {
                var paletteIndexes = NodeFactory.FromFile(nscr, FileOpenMode.Read)
                    .TransformWith<Binary2Nscr>()
                    .GetFormatAs<Nscr>()
                    .Maps
                    .Select(m => m.PaletteIndex)
                    .Distinct()
                    .ToArray();

                // Fill of palettes, even if we don't copy them so the palette
                // indexes are the same in the new collection
                var limitedPalette = new PaletteCollection();
                int maxIndex = paletteIndexes.Max();
                for (int i = 0; i <= maxIndex; i++) {
                    limitedPalette.Palettes.Add(new Palette());
                }

                // Copy only those palettes used in the original NSCR.
                for (int i = 0; i < paletteIndexes.Length; i++) {
                    int idx = paletteIndexes[i];
                    limitedPalette.Palettes[idx] = palette.Palettes[idx];
                }

                palette = limitedPalette;
            }

            IIndexedImage mergedImage = null;
            if (append && File.Exists(outNcgr)) {
                mergedImage = NodeFactory.FromFile(outNcgr, FileOpenMode.Read)
                    .TransformWith<Binary2Ncgr>()
                    .GetFormatAs<Ncgr>();
            }

            var compressionParams = new FullImageMapCompressionParams {
                MergeImage = mergedImage,
                Palettes = palette,
            };

            var compressed = NodeFactory.FromFile(input, FileOpenMode.Read)
                .TransformWith<Bitmap2FullImage>()
                .TransformWith<FullImageMapCompression, FullImageMapCompressionParams>(compressionParams);
            var newImage = compressed.Children[0].GetFormatAs<IndexedImage>();
            var map = compressed.Children[1].GetFormatAs<ScreenMap>();

            Ncgr newNcgr;
            if (ncgr == null) {
                newNcgr = new Ncgr(newImage);
            } else {
                var originalNcgr = NodeFactory.FromFile(ncgr, FileOpenMode.Read)
                    .TransformWith<Binary2Ncgr>()
                    .GetFormatAs<Ncgr>();
                newNcgr = new Ncgr(originalNcgr, newImage);
            }

            using var binary = new Ncgr2Binary().Convert(newNcgr);
            binary.Stream.WriteTo(outNcgr);

            Nscr newNscr;
            if (nscr == null) {
                newNscr = new Nscr(map);
            } else {
                var originalNscr = NodeFactory.FromFile(nscr, FileOpenMode.Read)
                    .TransformWith<Binary2Nscr>()
                    .GetFormatAs<Nscr>();
                newNscr = new Nscr(originalNscr, map);
            }

            using var binaryNscr = new Nscr2Binary().Convert(newNscr);
            binaryNscr.Stream.WriteTo(outNscr);
        }
    }
}
