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
namespace Texim.Tool.Megaman
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using Texim.Colors;
    using Texim.Compressions.Nitro;
    using Texim.Formats;
    using Texim.Palettes;
    using Texim.Pixels;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    public static class CommandLine
    {
        public static Command CreateCommand()
        {
            Command CreateExportCommand(string name, Action<string, string, string, string> exporter)
            {
                var command = new Command(name, $"Export {name}") {
                    new Option<string>("--palette", "the palette file", ArgumentArity.ExactlyOne),
                    new Option<string>("--pixels", "the pixels info file", ArgumentArity.ExactlyOne),
                    new Option<string>("--map", "the map file", ArgumentArity.ExactlyOne),
                    new Option<string>("--output", "the output file", ArgumentArity.ExactlyOne),
                };
                command.Handler = CommandHandler.Create(exporter);
                return command;
            }

            return new Command("megaman", "MegaMan NDS game") {
                new Command("export", "Export images") {
                    CreateExportCommand("title_local", ExportTitle),
                    CreateExportCommand("capcomlogo_local", ExportCapcomlogo),
                },
            };
        }

        private static void ExportCapcomlogo(string palette, string pixels, string map, string output)
        {
            // Image format with three RAW files:
            // - Palette: 16 palettes of 16 colors BGR555
            // - Pixels: indexed 4BPP with tile swizzling
            // - ScreenMap: 256x192
            // The game load the tiles in the VRAM at the offset 64. That is, it
            // shifts the tiles one tile. For that reason, the map points to the
            // image tiles starting at tile 1. It doesn't use the unset tile 0.
            // To export, instead of shiting the tiles, we substract to the map 1.
            var paletteParams = new RawPaletteParams {
                ColorEncoding = Bgr555.Instance,
                ColorsPerPalette = 16,
            };
            var pixelsParams = new RawIndexedImageParams {
                PixelEncoding = Indexed4Bpp.Instance,
                Swizzling = new TileSwizzling<IndexedPixel>(8),
            };
            var mapsParams = new RawScreenMapParams {
                Width = 256,
                Height = 192,
            };

            var paletteNode = NodeFactory.FromFile(palette, FileOpenMode.Read)
                .TransformWith<RawBinary2PaletteCollection, RawPaletteParams>(paletteParams);

            var pixelsNode = NodeFactory.FromFile(pixels, FileOpenMode.Read)
                .TransformWith<RawBinary2IndexedImage, RawIndexedImageParams>(pixelsParams);

            var mapsNode = NodeFactory.FromFile(map, FileOpenMode.Read)
                .TransformWith<RawBinary2ScreenMap, RawScreenMapParams>(mapsParams);

            // Required as the first tile in the map is 1.
            var screenMap = mapsNode.GetFormatAs<ScreenMap>();
            for (int i = 0; i < screenMap.Maps.Length; i++) {
                screenMap.Maps[i] = new MapInfo {
                    TileIndex = (short)(screenMap.Maps[i].TileIndex - 1),
                };
            }

            // Export them as an image
            var decompressionParams = new MapDecompressionParams {
                Map = screenMap,
            };
            var indexedImageParams = new IndexedImageBitmapParams {
                Palettes = paletteNode.GetFormatAs<PaletteCollection>(),
            };
            pixelsNode.TransformWith<MapDecompression, MapDecompressionParams>(decompressionParams)
                .TransformWith<IndexedImage2Bitmap, IndexedImageBitmapParams>(indexedImageParams)
                .Stream.WriteTo(output);
        }

        private static void ExportTitle(string palette, string pixels, string map, string output)
        {
            // Image format with three RAW files:
            // - Palette: 1 palette of 256 colors BGR555
            // - Pixels: indexed 8BPP with tile swizzling
            // - ScreenMap: 256x192
            // For some reason the map uses the tile 0xFF which is not present
            // in the image. We set the parameter "OutOfBoundsTileIndex" to use
            // instead the tile 0.
            var paletteParams = new RawPaletteParams {
                ColorEncoding = Bgr555.Instance,
            };
            var pixelsParams = new RawIndexedImageParams {
                PixelEncoding = Indexed8Bpp.Instance,
                Swizzling = new TileSwizzling<IndexedPixel>(8),
            };
            var mapsParams = new RawScreenMapParams {
                Width = 256,
                Height = 192,
            };

            var paletteNode = NodeFactory.FromFile(palette, FileOpenMode.Read)
                .TransformWith<RawBinary2PaletteCollection, RawPaletteParams>(paletteParams);

            var pixelsNode = NodeFactory.FromFile(pixels, FileOpenMode.Read)
                .TransformWith<RawBinary2IndexedImage, RawIndexedImageParams>(pixelsParams);

            var mapsNode = NodeFactory.FromFile(map, FileOpenMode.Read)
                .TransformWith<RawBinary2ScreenMap, RawScreenMapParams>(mapsParams);

            // Export them as an image
            var decompressionParams = new MapDecompressionParams {
                Map = mapsNode.GetFormatAs<ScreenMap>(),
                OutOfBoundsTileIndex = 0,
            };
            var indexedImageParams = new IndexedImageBitmapParams {
                Palettes = paletteNode.GetFormatAs<PaletteCollection>(),
            };
            pixelsNode.TransformWith<MapDecompression, MapDecompressionParams>(decompressionParams)
                .TransformWith<IndexedImage2Bitmap, IndexedImageBitmapParams>(indexedImageParams)
                .Stream.WriteTo(output);
        }
    }
}
