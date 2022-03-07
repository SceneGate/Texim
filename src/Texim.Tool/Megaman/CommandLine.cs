// Copyright (c) 2022 SceneGate

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
namespace Texim.Tool.Megaman;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using Texim.Formats;
using Texim.Images;
using Texim.Palettes;
using Texim.Sprites;
using Yarhl.FileSystem;
using Yarhl.IO;

public static class CommandLine
{
    public static Command CreateCommand()
    {
        var exportSprite = new Command("export_sprite", "Export sprites") {
                new Option<string>("--input", "the .spr sprite file", ArgumentArity.ExactlyOne),
                new Option<string>("--output", "the output directory", ArgumentArity.ExactlyOne),
                new Option<StandardPaletteFormat>("--format", "the output format", ArgumentArity.ExactlyOne),
            };
        exportSprite.Handler = CommandHandler.Create<string, string, StandardPaletteFormat>(ExportSprite);

        return new Command("megaman", "Megaman NDS game") {
            exportSprite,
        };
    }

    public static void ExportSprite(string input, string output, StandardPaletteFormat format)
    {
        var node = NodeFactory.FromFile(input, FileOpenMode.Read)
            .TransformWith<BinarySpr2Sprite>();

        var image = node.Children["pixels"].GetFormatAs<IndexedImage>();
        var palettes = node.Children["palettes"].GetFormatAs<PaletteCollection>();

        var spriteParams = new ImageSegment2IndexedImageParams {
            FullImage = image,
            TileSize = new System.Drawing.Size(8, 8),
        };
        var bitmapParams = new IndexedImageBitmapParams {
            Palettes = palettes,
        };

        foreach (var animation in node.Children["animations"].Children) {
            string name = animation.Name;
            foreach (var frame in animation.Children) {
                string outputFile = Path.Combine(output, name, $"{frame.Name}.png");
                frame.TransformWith<Sprite2IndexedImage, ImageSegment2IndexedImageParams>(spriteParams)
                    .TransformWith<IndexedImage2Bitmap, IndexedImageBitmapParams>(bitmapParams)
                    .Stream.WriteTo(outputFile);
            }
        }
    }
}
