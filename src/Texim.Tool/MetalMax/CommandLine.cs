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
namespace Texim.Tool.MetalMax
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using Texim.Formats;
    using Texim.Images;
    using Texim.Processing;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    public static class CommandLine
    {
        public static Command CreateCommand()
        {
            var export = new Command("export", "Export image") {
                new Option<string>("input", "the input file"),
                new Option<string>("output", "the output image file"),
            };
            export.Handler = CommandHandler.Create<string, string>(ExportImage);

            var import = new Command("import", "Import image") {
                new Option<string>("original", "the original image"),
                new Option<string>("input", "the new input image"),
                new Option<string>("output", "the output file"),
            };
            import.Handler = CommandHandler.Create<string, string, string>(ImportImage);

            return new Command("metalmax", "Metal Max NDS game") {
                export,
            };
        }

        private static void ExportImage(string input, string output)
        {
            using Node node = NodeFactory.FromFile(input, FileOpenMode.Read)
                .TransformWith<Binary2MmTex>();

            var parameters = new IndexedImageBitmapParams {
                Palette = node.GetFormatAs<MmTex>(),
            };
            node.TransformWith<IndexedImage2Bitmap, IndexedImageBitmapParams>(parameters)
                .Stream.WriteTo(output);
        }

        private static void ImportImage(string original, string input, string output)
        {
            MmTex originalTex = NodeFactory.FromFile(original, FileOpenMode.Read)
                .TransformWith<Binary2MmTex>()
                .GetFormatAs<MmTex>();

            var quantization = new FixedPaletteQuantization(originalTex);
            using var newNode = NodeFactory.FromFile(input, FileOpenMode.Read)
                .TransformWith<Bitmap2FullImage>()
                .TransformWith<FullImage2IndexedPalette, IQuantization>(quantization);

            IIndexedImage newImage = newNode.GetFormatAs<IIndexedImage>();
            var newMmtex = new MmTex {
                Colors = originalTex.Colors,
                Pixels = newImage.Pixels,
                Width = newImage.Width,
                Height = newImage.Height,
                NumImages = originalTex.NumImages,
                Unknown = originalTex.Unknown,
                UnknownSize = originalTex.UnknownSize,
            };

            newNode.ChangeFormat(newMmtex);
            newNode.TransformWith<Binary2MmTex>().Stream.WriteTo(output);
        }
    }
}
