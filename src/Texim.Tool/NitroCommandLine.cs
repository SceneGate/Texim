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
namespace Texim.Tool
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Colors;
    using Pixels;
    using Texim.Compressions.Nitro;
    using Texim.Formats;
    using Texim.Games.Nitro;
    using Texim.Images;
    using Texim.Palettes;
    using Texim.Processing;
    using Texim.Sprites;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    public static class NitroCommandLine
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

            var exportSprite = new Command("export_sprite", "Export Nitro files into an sprite image") {
                new Option<string>("--nclr", "nitro palette file", ArgumentArity.ExactlyOne),
                new Option<string>("--ncgr", "nitro pixel file", ArgumentArity.ExactlyOne),
                new Option<string>("--ncer", "nitro cell file", ArgumentArity.ZeroOrOne),
                new Option<string>("--output", "Output folder", ArgumentArity.ExactlyOne),
                new Option<StandardImageFormat>("format", "Output image format", ArgumentArity.ZeroOrOne),
            };
            exportSprite.Handler =
                CommandHandler.Create<string, string, string, string, StandardImageFormat>(ExportSprite);

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

            var importSprite = new Command("import_sprite", "Import an sprite as Nitro format") {
                new Option<string>("--input", "the input directory with the images", ArgumentArity.ExactlyOne),
                new Option<string>("--nclr", "the palette to use", ArgumentArity.ExactlyOne),
                new Option<string>("--ncgr", "optional original NCGR to copy params", ArgumentArity.ZeroOrOne),
                new Option<string>("--ncer", "optional original NCER to copy params", ArgumentArity.ZeroOrOne),
                new Option<string>("--out-ncgr", "the output nitro image file", ArgumentArity.ExactlyOne),
                new Option<string>("--out-ncer", "the output nitro sprite file", ArgumentArity.ExactlyOne),
            };
            importSprite.Handler = CommandHandler.Create<string, string, string, string, string, string>(ImportSprites);

            return new Command("nitro", "Nintendo DS standard formats") {
                exportPalette,
                exportImage,
                exportSprite,
                importImage,
                importCompressedImage,
                importSprite,
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
                Encoder = format.GetEncoder(),
            };
            pixels.TransformWith<IndexedImage2Bitmap, IndexedImageBitmapParams>(indexedParams)
                .Stream.WriteTo(output);
        }

        private static void ExportSprite(
            string nclr,
            string ncgr,
            string ncer,
            string output,
            StandardImageFormat format)
        {
            var palette = NodeFactory.FromFile(nclr, FileOpenMode.Read)
                .TransformWith<Binary2Nclr>()
                .GetFormatAs<Nclr>();

            var pixels = NodeFactory.FromFile(ncgr, FileOpenMode.Read)
                .TransformWith<Binary2Ncgr>()
                .GetFormatAs<Ncgr>() !;

            var indexedParams = new IndexedImageBitmapParams {
                Palettes = palette,
                Encoder = format.GetEncoder(),
            };

            var sprites = NodeFactory.FromFile(ncer, FileOpenMode.Read)
                .TransformWith<Binary2Ncer>();
            var spriteParams = new Sprite2IndexedImageParams {
                FullImage = pixels,
                RelativeCoordinates = SpriteRelativeCoordinatesKind.Center,
                IsTiled = pixels.IsTiled,
            };

            Console.WriteLine($"Exporting {sprites.Children.Count} sprites");
            foreach (Node sprite in sprites.Children) {
                if (sprite.GetFormatAs<Cell>() !.Segments.Count == 0) {
                    continue;
                }

                string outputImage = Path.Combine(output, sprite.Name + ".png");
                sprite.TransformWith<Sprite2IndexedImage, Sprite2IndexedImageParams>(spriteParams)
                    .TransformWith<IndexedImage2Bitmap, IndexedImageBitmapParams>(indexedParams)
                    .Stream!.WriteTo(outputImage);
            }
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

        private static void ImportSprites(string input, string nclr, string ncgr, string ncer, string outNcgr, string outNcer)
        {
            IPaletteCollection palette = NodeFactory.FromFile(nclr, FileOpenMode.Read)
                .TransformWith<Binary2Nclr>()
                .GetFormatAs<Nclr>();

            Ncgr image = NodeFactory.FromFile(ncgr, FileOpenMode.Read)
                .TransformWith<Binary2Ncgr>()
                .GetFormatAs<Ncgr>() !;

            var mergeImageSwizzling = new TileSwizzling<IndexedPixel>(image.Width);
            Memory<IndexedPixel> initialTiles = mergeImageSwizzling.Swizzle(image.Pixels).AsMemory();

            var tileSize = new Size(8, 8);
            int pixelsPerTile = tileSize.Width * tileSize.Height;

            // Add the initial tiles in case of creating one pixel data + several maps
            var uniqueTiles = new List<Memory<IndexedPixel>>();
            for (int i = 0; i < initialTiles.Length; i += pixelsPerTile) {
                Memory<IndexedPixel> tile = initialTiles.Slice(i, pixelsPerTile);
                uniqueTiles.Add(tile);
            }

            Ncer sprites = NodeFactory.FromFile(ncer, FileOpenMode.Read)
                .TransformWith<Binary2Ncer>()
                .GetFormatAs<Ncer>() !;

            var segmentation = new ImageSegmentation();

            foreach (string newImagePath in Directory.GetFiles(input)) {
                // This method will import only matching sprites. It will not replace / remove existing tiles but always
                // append. As a consequence the image maybe bigger than the VRAM accepts.
                string name = Path.GetFileNameWithoutExtension(newImagePath);
                int index = int.Parse(name["cell".Length..]);
                if (index > sprites.Root.Children.Count) {
                    throw new InvalidOperationException("Index too large");
                }

                FullImage newImage = NodeFactory.FromFile(newImagePath, FileOpenMode.Read)
                    .TransformWith<Bitmap2FullImage>()
                    .GetFormatAs<FullImage>() !;

                // First replace sprite definition (Cell and OAMs)
                (Sprite newSprite, FullImage newImageTrimmed) = segmentation.Segment(newImage);

                Cell originalCell = sprites.Root.Children[index].GetFormatAs<Cell>() !;
                var newCell = new Cell(originalCell, newSprite.Segments) {
                    Width = newSprite.Width,
                    Height = newSprite.Height,
                };

                sprites.Root.Children[index].ChangeFormat(newCell);

                // Now add the new tiles to the image.
                foreach (ObjectAttributeMemory obj in newCell.Segments) {
                    // We need to quantize each subimage with the best palette.
                    Rgb[] subImage = new Rgb[0];

                    // We simulate like the subimage is a big tile for the quantization
                    var quantization = new FixedPaletteTileQuantization(
                        palette,
                        new Size(obj.Width, obj.Height),
                        obj.Width);
                    quantization.FirstAsTransparent = true;
                    (IndexedPixel[] pixels, _) = quantization.Quantize(subImage);

                    // Now find unique pixels
                    var swizzling = new TileSwizzling<IndexedPixel>(obj.Width);
                    Memory<IndexedPixel> tiles = swizzling.Swizzle(pixels).AsMemory();
                    int numTiles = tiles.Length / pixelsPerTile;

                    for (int i = 0; i < numTiles; i++) {
                        var tile = tiles.Slice(i * pixelsPerTile, pixelsPerTile);
                        int repeatedIdx = uniqueTiles.FindIndex(t => t.HasEquivalentIndexes(tile));
                        if (repeatedIdx != -1) {
                            obj.HorizontalFlip = false;
                            obj.VerticalFlip = false;
                            obj.TileIndex = (short)repeatedIdx;
                            obj.PaletteIndex = pixels[0].PaletteIndex;
                            continue;
                        }

                        tile.FlipHorizontal(tileSize);
                        repeatedIdx = uniqueTiles.FindIndex(t => t.HasEquivalentIndexes(tile));
                        if (repeatedIdx != -1) {
                            obj.HorizontalFlip = true;
                            obj.VerticalFlip = false;
                            obj.TileIndex = (short)repeatedIdx;
                            obj.PaletteIndex = pixels[0].PaletteIndex;
                            continue;
                        }

                        tile.FlipVertical(tileSize);
                        repeatedIdx = uniqueTiles.FindIndex(t => t.HasEquivalentIndexes(tile));
                        if (repeatedIdx != -1) {
                            obj.HorizontalFlip = true;
                            obj.VerticalFlip = true;
                            obj.TileIndex = (short)repeatedIdx;
                            obj.PaletteIndex = pixels[0].PaletteIndex;
                            continue;
                        }

                        tile.FlipHorizontal(tileSize);
                        repeatedIdx = uniqueTiles.FindIndex(t => t.HasEquivalentIndexes(tile));
                        if (repeatedIdx != -1) {
                            obj.HorizontalFlip = false;
                            obj.VerticalFlip = true;
                            obj.TileIndex = (short)repeatedIdx;
                            obj.PaletteIndex = pixels[0].PaletteIndex;
                            continue;
                        }

                        tile.FlipVertical(tileSize);
                        obj.HorizontalFlip = false;
                        obj.VerticalFlip = false;
                        obj.TileIndex = (short)uniqueTiles.Count;
                        obj.PaletteIndex = pixels[0].PaletteIndex;
                        uniqueTiles.Add(tile);
                    }
                }
            }

            var uniquePixels = uniqueTiles.SelectMany(t => t.ToArray()).ToArray();
            var unswizzling = new TileSwizzling<IndexedPixel>(tileSize, 8);
            uniquePixels = unswizzling.Unswizzle(uniquePixels);

            image.Pixels = uniquePixels;
            image.Width = 8;
            image.Height = uniquePixels.Length / 8;

            using var newImageNode = new Node("image", image);
            newImageNode.TransformWith<Ncgr2Binary>().Stream!.WriteTo(outNcgr);

            using var newSpriteNode = new Node("sprites", sprites);
            newSpriteNode.TransformWith<Ncer2Binary>().Stream!.WriteTo(outNcer);
        }
    }
}
