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
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using Texim.Formats;
    using Texim.Games.DevilSurvivor;
    using Texim.Images;
    using Texim.Palettes;
    using Texim.Processing;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    public static class DevilSurvivorCommandLine
    {
        public static Command CreateCommand()
        {
            var exportImage = new Command("export", "Export an image") {
                new Option<string>("--palette", "the palette file", ArgumentArity.ExactlyOne),
                new Option<string>("--image", "the image file", ArgumentArity.ExactlyOne),
                new Option<string>("--output", "the output file", ArgumentArity.ExactlyOne),
            };
            exportImage.Handler = CommandHandler.Create<string, string, string>(ExportImage);

            var importImage = new Command("import", "Import an image") {
                new Option<string>("--palette", "the palette file", ArgumentArity.ExactlyOne),
                new Option<string>("--image", "the image file", ArgumentArity.ExactlyOne),
                new Option<string>("--output", "the output file", ArgumentArity.ExactlyOne),
            };
            importImage.Handler = CommandHandler.Create<string, string, string>(ImportImage);

            return new Command("devilsurvivor", "Devil Survivor NDS game") {
                exportImage,
                importImage,
            };
        }

        private static void ExportImage(string palette, string image, string output)
        {
            Node paletteNode = NodeFactory.FromFile(palette, FileOpenMode.Read)
                .TransformWith<DsTex2Palette>();

            var exporterParameters = new IndexedImageBitmapParams {
                Palette = paletteNode.GetFormatAs<Palette>(),
            };
            NodeFactory.FromFile(image, FileOpenMode.Read)
                .TransformWith<DsTex2Image>()
                .TransformWith(new IndexedImage2Bitmap(exporterParameters))
                .Stream.WriteTo(output);
        }

        private static void ImportImage(string palette, string image, string output)
        {
            Node paletteNode = NodeFactory.FromFile(palette, FileOpenMode.Read)
                .TransformWith<DsTex2Palette>();

            var quantization = new FixedPaletteQuantization(paletteNode.GetFormatAs<Palette>()) {
                TransparentIndex = 0x15,
            };
            NodeFactory.FromFile(image, FileOpenMode.Read)
                .TransformWith<Bitmap2FullImage>()
                .TransformWith(new FullImage2IndexedPalette(quantization))
                .TransformWith<DsTex2Image>()
                .Stream.WriteTo(output);
        }
    }
}
