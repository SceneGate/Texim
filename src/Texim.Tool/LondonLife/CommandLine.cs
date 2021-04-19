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
namespace Texim.Tool.LondonLife
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using Texim.Formats;
    using Texim.Palettes;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    public static class CommandLine
    {
        public static Command CreateCommand()
        {
            var exportAcl = new Command("export_acl", "Export ACL palettes") {
                new Option<string>("--input", "the input ACL file", ArgumentArity.ExactlyOne),
                new Option<string>("--output", "the output directory", ArgumentArity.ExactlyOne),
                new Option<StandardPaletteFormat>("--format", "the output format", ArgumentArity.ExactlyOne),
            };
            exportAcl.Handler = CommandHandler.Create<string, string, StandardPaletteFormat>(ExportAcl);

            var exportNccl = new Command("export_nccl", "Export NCCL palettes") {
                new Option<string>("--input", "the input NCCL file", ArgumentArity.ExactlyOne),
                new Option<string>("--output", "the output directory", ArgumentArity.ExactlyOne),
                new Option<StandardPaletteFormat>("--format", "the output format", ArgumentArity.ExactlyOne),
            };
            exportNccl.Handler = CommandHandler.Create<string, string, StandardPaletteFormat>(ExportNccl);

            return new Command("londonlife", "Professor Layton London Life NDS game") {
                exportAcl,
                exportNccl,
            };
        }

        public static void ExportAcl(string input, string output, StandardPaletteFormat format)
        {
            PaletteCollection collection = NodeFactory.FromFile(input, FileOpenMode.Read)
                .TransformWith<Acl2PaletteCollection>()
                .GetFormatAs<PaletteCollection>();

            string name = Path.GetFileNameWithoutExtension(input);
            ExportPaletteCollection(collection, name, output, format);
        }

        public static void ExportNccl(string input, string output, StandardPaletteFormat format)
        {
            PaletteCollection collection = NodeFactory.FromFile(input, FileOpenMode.Read)
                .TransformWith<Nccl2PaletteCollection>()
                .GetFormatAs<PaletteCollection>();

            string name = Path.GetFileNameWithoutExtension(input);
            ExportPaletteCollection(collection, name, output, format);
        }

        private static void ExportPaletteCollection(
            PaletteCollection collection,
            string name,
            string output,
            StandardPaletteFormat format)
        {
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

            for (int i = 0; i < collection.Palettes.Count; i++) {
                using BinaryFormat outputStream = converter.Convert(collection.Palettes[i]);

                string outputPath = Path.Combine(output, $"{name}_{i}.{extension}");
                outputStream.Stream.WriteTo(outputPath);
            }
        }
    }
}
