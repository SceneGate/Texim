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
                new Option<int>("--cell", () => -1, "optional cell to export only"),
            };
            exportSprite.Handler =
                CommandHandler.Create<string, string, string, string, StandardImageFormat, int>(ExportSprite);

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
                new Option<int>("--cell", () => -1, "optional cell to import only"),
            };
            importSprite.Handler = CommandHandler.Create<string, string, string, string, string, string, int>(ImportSprites);

            var testImportSprite = new Command("test_sprite_import") {
                new Option<string>("--nclr"),
                new Option<string>("--ncgr"),
                new Option<string>("--ncer"),
                new Option<string>("--temp"),
                new Option<int>("--cell"),
            };
            testImportSprite.Handler = CommandHandler.Create<string, string, string, string, int>(TestImportSprites);

            return new Command("nitro", "Nintendo DS standard formats") {
                exportPalette,
                exportImage,
                exportSprite,
                importImage,
                importCompressedImage,
                importSprite,
                testImportSprite,
            };
        }

        private static void TestImportSprites(string nclr, string ncgr, string ncer, string temp, int cell)
        {
            if (Directory.Exists(temp)) {
                Directory.Delete(temp, true);
            }

            string imagesPath = Path.Combine(temp, "images");
            ExportSprite(nclr, ncgr, ncer, imagesPath, StandardImageFormat.Png, cell);

            string binaryPath = Path.Combine(temp, "binary");
            string ncgrPath = Path.Combine(binaryPath, "new.ncgr");
            string ncerPath = Path.Combine(binaryPath, "new.ncer");
            ImportSprites(imagesPath, nclr, ncgr, ncer, ncgrPath, ncerPath, cell);

            string testImagesPath = Path.Combine(temp, "images_reeee");
            ExportSprite(nclr, ncgrPath, ncerPath, testImagesPath, StandardImageFormat.Png, cell);
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
            StandardImageFormat format,
            int cell)
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

            IEnumerable<Node> spritesToExport = sprites.Children;
            if (cell > -1) {
                spritesToExport = new Node[] { sprites.Children[cell] };
            }

            Console.WriteLine($"Exporting {spritesToExport.Count()} sprites");
            foreach (Node sprite in spritesToExport) {
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
                .GetFormatAs<Nclr>() !;

            if (useSamePalettes) {
                var paletteIndexes = NodeFactory.FromFile(nscr, FileOpenMode.Read)
                    .TransformWith<Binary2Nscr>()
                    .GetFormatAs<Nscr>() !
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

        private static void ImportSprites(string input, string nclr, string ncgr, string ncer, string outNcgr, string outNcer, int cell)
        {
            IPaletteCollection palette = NodeFactory.FromFile(nclr, FileOpenMode.Read)
                .TransformWith<Binary2Nclr>()
                .GetFormatAs<Nclr>();

            // Open the original NCGR as we will use its content as the initial image.
            // This means that we will only add the new (modified) tiles of the imported images
            // over the existing NCGR. It won't be built from scratch.
            // TODO: Create scratch NCGR if requested keeping only the metadata.
            Ncgr image = NodeFactory.FromFile(ncgr, FileOpenMode.Read)
                .TransformWith<Binary2Ncgr>()
                .GetFormatAs<Ncgr>() !;

            // We had to swizzle. We can't compare lineal pixels as there isn't
            // a constant "image width" that generates a valid sequence of lineal
            // pixels. Each segment has a different width. That's why NCGR usually
            // saves 0xFFFF as width. Having 8 as false width is the same as swizzling
            // tileSize.Width == imageWidth -> nop operation (but we need a copy).
            var imageSwizzler = new TileSwizzling<IndexedPixel>(image.Width);
            var uniqueTiledPixels = image.IsTiled
                ? imageSwizzler.Swizzle(image.Pixels).ToList()
                : image.Pixels.ToList();

            // Convert the lineal pixels into a list of tiles (groups of tileSize pixels)
            var tileSize = new Size(8, 8);
            int pixelsPerTile = tileSize.Width * tileSize.Height;

            Ncer sprites = NodeFactory.FromFile(ncer, FileOpenMode.Read)
                .TransformWith<Binary2Ncer>()
                .GetFormatAs<Ncer>() !;

            var segmentation = new ImageSegmentation();

            IEnumerable<string> files = Directory.GetFiles(input).OrderBy(x => x);
            if (cell > -1) {
                files = new List<string> { Path.Combine(input, $"cell{cell:D3}.png") };
            }

            foreach (string newImagePath in files) {
                // This method will import only matching sprites. It will not replace / remove existing tiles but always
                // append. As a consequence the image maybe bigger than the VRAM accepts.
                // The name MUST BE "cell<INDEX>".
                // This allows to not have to import ALL the images, but only the ones of interest.
                string name = Path.GetFileNameWithoutExtension(newImagePath);
                int index = int.Parse(name["cell".Length..]);
                Console.WriteLine($"Index: {index}");
                if (index > sprites.Root.Children.Count) {
                    throw new InvalidOperationException("Index too large");
                }

                FullImage newImage = NodeFactory.FromFile(newImagePath, FileOpenMode.Read)
                        .TransformWith<Bitmap2FullImage>()
                    .GetFormatAs<FullImage>() !;

                // First replace sprite definition (Cell and OAMs)
                // We keep the original metadata, just replace OAMs.
                (Sprite newSprite, FullImage _) = segmentation.Segment(newImage);

                Cell originalCell = sprites.Root.Children[index].GetFormatAs<Cell>() !;
                var newCell = new Cell(originalCell, newSprite.Segments) {
                    Width = newSprite.Width,
                    Height = newSprite.Height,
                };

                sprites.Root.Children[index].ChangeFormat(newCell);

                // Get the minimum pixel segment, in pixels as we are filling pixels
                int blockSize = sprites.TileMapping.GetTileBlockSize() * pixelsPerTile;
                if (image.Format == NitroTextureFormat.Indexed4Bpp) {
                    blockSize *= 2;
                }

                // Now add the new tiles to the image.
                foreach (ObjectAttributeMemory obj in newCell.Segments) {
                    // We need to quantize each subimage with the best palette. (RGB pixel -> index pixel as rest of NCGR)
                    // This will return the section of the image for the OAM.
                    Rgb[] subImage = SubImage(newImage.Pixels, newImage.Width, obj);

                    // We simulate like the subimage is a big tile for the quantization
                    // It will search for the best matching palette (e.g. cases of 16 palettes of 16 colors).
                    // LIMITATION: this may find a different palette than the original (case of palettes having same colors)
                    //    in that case it may not get the same indexes, so the compression will fail to find a matching tile.
                    //    Recommendation is to order colors in palettes, so even if they find different indexes are same.
                    // LIMITATION: We use the original palette, no new colors allowed.
                    var quantization = new FixedPaletteTileQuantization(
                        palette,
                        new Size(obj.Width, obj.Height),
                        obj.Width);
                    quantization.FirstAsTransparent = true;
                    var quantizationResult = (quantization.Quantize(subImage) as FixedPaletteTileQuantizationResult) !;

                    // OAMs can have only one palette index, that's why we use a big tile.
                    obj.PaletteIndex = quantizationResult.PaletteIndexes[0];
                    obj.HorizontalFlip = false; // not supported
                    obj.VerticalFlip = false; // not supported

                    IndexedPixel[] objTiles;
                    if (image.IsTiled) {
                        var segmentSwizzler = new TileSwizzling<IndexedPixel>(obj.Width);
                        objTiles = segmentSwizzler.Swizzle(quantizationResult.Pixels);
                    } else {
                        objTiles = quantizationResult.Pixels;
                    }

                    // Now find unique pixels.
                    // We don't need to find unique individual tiles but the full sequence of tiles of the OAM.
                    // and put the start index in the OAM.
                    int tileIndex = SearchSequence(uniqueTiledPixels, objTiles, pixelsPerTile, blockSize);
                    if (tileIndex == -1) {
                        if (objTiles.Length < blockSize) {
                            objTiles = objTiles.Concat(new IndexedPixel[blockSize - objTiles.Length]).ToArray();
                        }

                        // Add sequence to the pixels.
                        tileIndex = uniqueTiledPixels.Count / pixelsPerTile;
                        uniqueTiledPixels.AddRange(objTiles);
                    }

                    obj.PaletteMode = image.Format == NitroTextureFormat.Indexed8Bpp
                        ? NitroPaletteMode.Palette256x1
                        : NitroPaletteMode.Palette16x16;
                    obj.TileIndex = tileIndex;
                }
            }

            Console.WriteLine($"Previous pixels: {image.Pixels.Length}, new pixels: {uniqueTiledPixels.Count}");
            image.Pixels = image.IsTiled
                ? imageSwizzler.Unswizzle(uniqueTiledPixels)
                : uniqueTiledPixels.ToArray();
            image.Height = uniqueTiledPixels.Count / image.Width;

            using var newImageNode = new Node("image", image);
            newImageNode.TransformWith<Ncgr2Binary>().Stream!.WriteTo(outNcgr);

            using var newSpriteNode = new Node("sprites", sprites);
            newSpriteNode.TransformWith<Ncer2Binary>().Stream!.WriteTo(outNcer);
        }

        private static Rgb[] SubImage(Rgb[] image, int width, IImageSegment segment)
        {
            var subImage = new Rgb[segment.Width * segment.Height];
            int idx = 0;
            for (int y = 0; y < segment.Height; y++) {
                for (int x = 0; x < segment.Width; x++) {
                    int fullIndex = ((segment.CoordinateY + y + 128) * width) + segment.CoordinateX + x + 256;
                    subImage[idx++] = image[fullIndex];
                }
            }

            return subImage;
        }

        private static int SearchSequence(List<IndexedPixel> pixels, ReadOnlySpan<IndexedPixel> newPixels, int tileSize, int blockSize)
        {
            int tileNumber = -1;

            for (int tilePos = 0;
                 tilePos + blockSize < pixels.Count && tileNumber == -1;
                 tilePos += blockSize) {

                if (tilePos + newPixels.Length > pixels.Count) {
                    break;
                }

                tileNumber = tilePos / tileSize;
                if (HasSequence(pixels, newPixels, tilePos)) {
                    continue;
                }

                tileNumber = -1;
                // TODO: try again flipping.
            }

            return tileNumber;
        }

        private static bool HasSequence(List<IndexedPixel> pixels, ReadOnlySpan<IndexedPixel> newPixels, int tilePos)
        {
            bool hasSequence = true;
            for (int i = 0; i < newPixels.Length && hasSequence; i++) {
                if (pixels[tilePos + i].Index != newPixels[i].Index) {
                    hasSequence = false;
                }
            }

            return hasSequence;
        }
    }
}
