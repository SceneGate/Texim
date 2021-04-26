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
            var exportTitlelocal = new Command("title_local", "Export the image title local") {
                new Option<string>("--palette", "the palette file", ArgumentArity.ExactlyOne),
                new Option<string>("--pixels", "the pixels info file", ArgumentArity.ExactlyOne),
                new Option<string>("--map", "the map file", ArgumentArity.ExactlyOne),
                new Option<string>("--output", "the output file", ArgumentArity.ExactlyOne),
            };
            exportTitlelocal.Handler = CommandHandler.Create<string, string, string, string>(ExportTitleLocal);

            var export = new Command("export", "Export images") {
                exportTitlelocal,
            };
            return new Command("megaman", "MegaMan NDS game") {
                export,
            };
        }

        private static void ExportCapcomlogoLocal(string palette, string pixels, string map, string output)
        {
            // Read RAW files
            // Image: 4bpp tiled size 256x192 with maps starting at tile 1
            var paletteParams = new RawPaletteParams {
                ColorEncoding = Bgr555.Instance,

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
                .TransformWith<RawBinary2Palette, RawPaletteParams>(paletteParams);

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
                Palette = paletteNode.GetFormatAs<Palette>(),
            };
            pixelsNode.TransformWith<MapDecompression, MapDecompressionParams>(decompressionParams)
                .TransformWith<IndexedImage2Bitmap, IndexedImageBitmapParams>(indexedImageParams)
                .Stream.WriteTo(output);
        }

        private static void ExportTitleLocal(string palette, string pixels, string map, string output)
        {
            // Read RAW files
            // Image: 8bpp tiled size 256x192 with out bound tile index (0xFF -> 0x00)
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
                .TransformWith<RawBinary2Palette, RawPaletteParams>(paletteParams);

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
                Palette = paletteNode.GetFormatAs<Palette>(),
            };
            pixelsNode.TransformWith<MapDecompression, MapDecompressionParams>(decompressionParams)
                .TransformWith<IndexedImage2Bitmap, IndexedImageBitmapParams>(indexedImageParams)
                .Stream.WriteTo(output);
        }
    }
}
