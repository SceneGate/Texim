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
namespace Texim.Tool;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using Texim.Colors;
using Texim.Formats;
using Texim.Images;
using Texim.Palettes;
using Texim.Pixels;
using Texim.Processing;
using Yarhl.FileSystem;
using Yarhl.IO;

public static class DarkoCommandLine
{
    public static Command CreateCommand()
    {
        var testFont = new Command("test_font", "Export/Import font chars") {
            new Option<string>("--input", "the font char file", ArgumentArity.ExactlyOne),
            new Option<int>("--width", "the font char width", ArgumentArity.ExactlyOne),
            new Option<int>("--height", "the font char height", ArgumentArity.ExactlyOne),
        };
        testFont.Handler = CommandHandler.Create<string, int, int>(TestFont);

        return new Command("darko", "Do tests for Darkmet") {
            testFont,
        };
    }

    private static void TestFont(string input, int width, int height)
    {
        var palette = new Palette();
        for (int i = 0; i < 16; i++) {
            var color = new Rgb((byte)(i * 16), (byte)(i * 16), (byte)(i * 16));
            palette.Colors.Add(color);
        }

        // Export
        using var stream = DataStreamFactory.FromFile(input, FileOpenMode.Read);
        var reader = new DataReader(stream);

        var pixels = reader.ReadPixels<Indexed4BppBigEndian>(width * height);
        var image = new IndexedImage {
            Pixels = pixels,
            Width = width,
            Height = height,
        };
        var fullImage = image.CreateFullImage(palette);

        string directory = Path.GetDirectoryName(input);
        string name = Path.GetFileNameWithoutExtension(input);
        string output = Path.Combine(directory, name + ".png");
        using var binaryPng = new FullImage2Bitmap().Convert(fullImage);
        binaryPng.Stream.WriteTo(output);

        // Import back
        var quantization = new FixedPaletteQuantization(palette);
        var bitmapNode = NodeFactory.FromFile(output, FileOpenMode.Read)
            .TransformWith<Bitmap2FullImage>()
            .TransformWith<FullImage2IndexedPalette, IQuantization>(quantization);
        var newPixels = bitmapNode.GetFormatAs<IIndexedImage>().Pixels;

        using var newStream = new DataStream();
        var writer = new DataWriter(newStream);
        writer.Write<Indexed4BppBigEndian>(newPixels);
        newStream.WriteTo(Path.Combine(directory, name + ".nbin"));

        // Compare
        Console.WriteLine($"Is Darko happy? {newStream.Compare(stream)}");
    }
}
