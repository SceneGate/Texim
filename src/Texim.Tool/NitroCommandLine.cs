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
    using System.Reflection;
    using Texim.Colors;
    using Texim.Compressions.Nitro;
    using Texim.Formats;
    using Texim.Games.Nitro;
    using Texim.Images;
    using Texim.Palettes;
    using Texim.Pixels;
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
            };
            importSprite.Handler = CommandHandler.Create<string, string, string, string, string, string>(ImportSprites);

            var testImportSprite = new Command("test_sprite_import") {
                new Option<string>("--nclr"),
                new Option<string>("--ncgr"),
                new Option<string>("--ncer"),
                new Option<string>("--temp"),
                new Option<int>("--cell", () => -1),
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
            ImportSprites(imagesPath, nclr, ncgr, ncer, ncgrPath, ncerPath);

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
                _ = pixels.TransformWith<MapDecompression, MapDecompressionParams>(mapParams);
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
                .GetFormatAs<Ncgr>()!;

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
                if (sprite.GetFormatAs<Cell>()!.Segments.Count == 0) {
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
                .GetFormatAs<Nclr>()!;

            if (useSamePalettes) {
                byte[] paletteIndexes = NodeFactory.FromFile(nscr, FileOpenMode.Read)
                    .TransformWith<Binary2Nscr>()
                    .GetFormatAs<Nscr>()!
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

        private static void ImportSprites(
            string input,
            string nclr,
            string ncgr,
            string ncer,
            string outNcgr,
            string outNcer)
        {
            // We use the original palette rather than creating a new from the image.
            // FUTURE: save palettes from quantization output.
            // Not an easy task as we have to figure out the best combination of 16 sets of 16 colors.
            using Node paletteNode = NodeFactory.FromFile(nclr, FileOpenMode.Read)
                .TransformWith<Binary2Nclr>();
            Nclr palette = paletteNode.GetFormatAs<Nclr>()!;

            // Open the original NCGR and NCER for the metadata and
            // to take the pixels from the sprite cells that won't be imported.
            using Node imageNode = NodeFactory.FromFile(ncgr, FileOpenMode.ReadWrite)
                .TransformWith<Binary2Ncgr>();
            Ncgr image = imageNode.GetFormatAs<Ncgr>()!;

            var tileSize = new Size(8, 8); // NCGR images always have a tile size of 8x8.
            int pixelsPerTile = tileSize.Width * tileSize.Height;

            using Node spriteCollectionNode = NodeFactory.FromFile(ncer, FileOpenMode.ReadWrite)
                .TransformWith<Binary2Ncer>();
            Ncer spriteCollection = spriteCollectionNode.GetFormatAs<Ncer>()!;
            int numSprites = spriteCollectionNode.Children.Count;

            // Get the minimum pixel segment, in pixels as we are filling pixels.
            int blockSize = spriteCollection.TileMapping.GetTileBlockSize() * pixelsPerTile;
            if (image.Format == NitroTextureFormat.Indexed4Bpp) {
                blockSize *= 2;
            }

            var uniqueTileSequences = new List<IndexedPixel>();

            for (int cellIndex = 0; cellIndex < numSprites; cellIndex++) {
                string cellPath = Path.Combine(input, $"cell{cellIndex:D3}.png");
                Cell originalCell = spriteCollectionNode.Children[cellIndex].GetFormatAs<Cell>()!;

                // Find the sprite images that we will be importing. Some NCER may have hundreds (>300)
                // sprites, so some times you will want to import a subset.
                // The name MUST BE "cell<INDEX>". This is the name the Binary2Ncer gives to its children.
                if (File.Exists(cellPath)) {
                    Console.WriteLine($"  - Importing cell {cellIndex}");
                    var newCell = ImportModifiedCell(cellPath, blockSize, pixelsPerTile, originalCell, uniqueTileSequences, palette, image);
                    spriteCollectionNode.Children[cellIndex].ChangeFormat(newCell);
                } else {
                    Console.WriteLine($"  - Re-using cell {cellIndex}");
                    ImportOriginalCell(originalCell, blockSize, pixelsPerTile, uniqueTileSequences, image);
                }
            }

            Console.WriteLine($"Previous pixels: {image.Pixels.Length}, new pixels: {uniqueTileSequences.Count}");
            var imageSwizzler = new TileSwizzling<IndexedPixel>(image.Width);
            image.Pixels = image.IsTiled
                ? imageSwizzler.Unswizzle(uniqueTileSequences)
                : uniqueTileSequences.ToArray();
            image.Height = uniqueTileSequences.Count / image.Width;

            // Serialize into the files
            imageNode.TransformWith<Ncgr2Binary>()
                .Stream!.WriteTo(outNcgr);

            spriteCollectionNode.TransformWith<Ncer2Binary>()
                .Stream!.WriteTo(outNcer);
        }

        private static void ImportOriginalCell(Cell originalCell, int blockSize, int pixelsPerTile, List<IndexedPixel> uniqueTileSequences, Ncgr image)
        {
            foreach (ObjectAttributeMemory segment in originalCell.Segments.Cast<ObjectAttributeMemory>()) {
                // No need to swizzle because the next methods retrieves the tiles in the same format from the image.
                IndexedPixel[] segmentTiles = GetSegmentTiles(image.Pixels, segment);

                // Now find unique pixels.
                // We don't need to find unique individual tiles but the full sequence of tiles of the OAM.
                // and put the start index in the OAM.
                int tileIndex = SearchSequence(uniqueTileSequences, segmentTiles, blockSize);
                if (tileIndex == -1) {
                    if (segmentTiles.Length < blockSize) {
                        segmentTiles = segmentTiles.Concat(new IndexedPixel[blockSize - segmentTiles.Length]).ToArray();
                    }

                    // Add sequence to the pixels.
                    segment.TileIndex = uniqueTileSequences.Count / pixelsPerTile;
                    uniqueTileSequences.AddRange(segmentTiles);
                } else {
                    segment.TileIndex = tileIndex / pixelsPerTile;
                }
            }
        }

        private static IndexedPixel[] GetSegmentTiles(IndexedPixel[] source, IImageSegment segment)
        {
            var segmentPixels = new IndexedPixel[segment.Width * segment.Height];
            Span<IndexedPixel> pixelsOut = segmentPixels;
            Span<IndexedPixel> pixelsIn = source;

            var tileSize = new Size(8, 8);
            int pixelsPerTile = tileSize.Width * tileSize.Height;
            int numPixels = segment.Width * segment.Height;

            int startIndex = segment.TileIndex * pixelsPerTile;
            if (startIndex + numPixels > pixelsIn.Length || startIndex < 0) {
                throw new FormatException($"Required tile index {segment.TileIndex} is out of bounds");
            }

            Span<IndexedPixel> tilesIn = pixelsIn.Slice(startIndex, numPixels);
            for (int t = 0; t < numPixels; t++) {
                pixelsOut[t] = new IndexedPixel(
                    tilesIn[t].Index,
                    tilesIn[t].Alpha,
                    segment.PaletteIndex);
            }

            return segmentPixels;
        }

        private static Cell ImportModifiedCell(
            string spriteImagePath,
            int blockSize,
            int pixelsPerTile,
            Cell originalCell,
            List<IndexedPixel> uniqueTileSequences,
            Nclr palette,
            Ncgr image)
        {
            // Open the new image file (without palette quantization).
            using var binaryNewImage = new BinaryFormat(DataStreamFactory.FromFile(spriteImagePath, FileOpenMode.Read));
            var bitmap2FullImage = new Bitmap2FullImage();
            FullImage newImage = bitmap2FullImage.Convert(binaryNewImage);

            // First segment the cell into smaller parts (OAMs).
            // FUTURE: Check if the image fits in the existing cells, if so, re-use
            // It's hard to segment properly our image as the original source image
            // would have proper size (not full canvas) and proper layers. If we can
            // re-use the original cells we will achieve better tile re-using.
            var segmentation = new ImageSegmentation();
            (Sprite newSprite, _) = segmentation.Segment(newImage);

            // Create new cell from the metadata of the original.
            var newCell = new Cell(originalCell, newSprite.Segments) {
                Width = newSprite.Width,
                Height = newSprite.Height,
            };

            // Now over its segments (OAMs), let's update its tile index and
            // finding if there are matching sequences. Add them the new to the NCGR image.
            foreach (ObjectAttributeMemory segment in newCell.Segments.Cast<ObjectAttributeMemory>()) {
                // Tiles are indexed pixels. We need to quantize first that section of the image.
                Rgb[] segmentImage = SubImage(newImage.Pixels, newImage.Width, segment);

                // We simulate like the subimage is a big tile for the quantization so it finds only one palette.
                // It will search for the best matching palette (e.g. cases of 16 palettes of 16 colors).
                // LIMITATION: this may find a different palette than the original (case of palettes having same colors)
                //    in that case it may not get the same indexes, so the compression will fail to find a matching tile.
                //    Recommendation is to order colors in palettes, so even if they find different indexes are same.
                // LIMITATION: We use the original palette, no new colors allowed.
                var quantization = new FixedPaletteTileQuantization(
                    palette,
                    new Size(segment.Width, segment.Height),
                    segment.Width);
                quantization.FirstAsTransparent = true;
                var quantizationResult = (quantization.Quantize(segmentImage) as FixedPaletteTileQuantizationResult)!;

                // OAMs can have only one palette index, that's why we use a big tile.
                segment.PaletteIndex = quantizationResult.PaletteIndexes[0];
                segment.HorizontalFlip = false; // not supported
                segment.VerticalFlip = false; // not supported

                // We had to swizzle. We can't compare lineal pixels as there isn't
                // a constant "image width" that generates a valid sequence of lineal
                // pixels. Each segment has a different width. That's why NCGR usually
                // saves 0xFFFF as width. Having 8 as false width is the same as swizzling
                // tileSize.Width == imageWidth -> nop operation (but we need a copy).
                IndexedPixel[] segmentTiles;
                if (image.IsTiled) {
                    var segmentSwizzler = new TileSwizzling<IndexedPixel>(segment.Width);
                    segmentTiles = segmentSwizzler.Swizzle(quantizationResult.Pixels);
                } else {
                    segmentTiles = quantizationResult.Pixels;
                }

                // Now find unique pixels.
                // We don't need to find unique individual tiles but the full sequence of tiles of the OAM.
                // and put the start index in the OAM.
                // Even if the image is not tiled, the OAM saves the index divided by tiles.
                int tileIndex = SearchSequence(uniqueTileSequences, segmentTiles, blockSize);
                if (tileIndex == -1) {
                    if (segmentTiles.Length < blockSize) {
                        segmentTiles = segmentTiles.Concat(new IndexedPixel[blockSize - segmentTiles.Length]).ToArray();
                    }

                    // Add sequence to the pixels.
                    segment.TileIndex = uniqueTileSequences.Count / pixelsPerTile;
                    uniqueTileSequences.AddRange(segmentTiles);
                } else {
                    segment.TileIndex = tileIndex / pixelsPerTile;
                }

                segment.PaletteMode = image.Format == NitroTextureFormat.Indexed8Bpp
                    ? NitroPaletteMode.Palette256x1
                    : NitroPaletteMode.Palette16x16;
            }

            return newCell;
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

        private static int SearchSequence(List<IndexedPixel> pixels, ReadOnlySpan<IndexedPixel> newPixels, int blockSize)
        {
            int foundPos = -1;

            for (int current = 0; current + blockSize < pixels.Count && foundPos == -1; current += blockSize) {
                if (current + newPixels.Length > pixels.Count) {
                    break;
                }

                foundPos = current;
                if (HasSequence(pixels, newPixels, current)) {
                    continue;
                }

                // TODO: try again flipping.
                foundPos = -1;
            }

            return foundPos;
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
