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
namespace Texim.Formats
{
    using System;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.Formats.Png;
    using SixLabors.ImageSharp.PixelFormats;
    using Texim.Palettes;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public class Palette2Bitmap : IConverter<IPalette, BinaryFormat>
    {
        private const int ColorsPerRow = 16;
        private const int ZoomSize = 10;
        private readonly IImageEncoder encoder;

        public Palette2Bitmap()
        {
            encoder = new PngEncoder();
        }

        public Palette2Bitmap(IImageEncoder parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);
            encoder = parameters;
        }

        public BinaryFormat Convert(IPalette source)
        {
            ArgumentNullException.ThrowIfNull(source);

            var colors = source.Colors;

            int width = ColorsPerRow * ZoomSize;
            int height = (int)Math.Ceiling((float)colors.Count / ColorsPerRow);
            height *= ZoomSize;

            using var image = new Image<Rgba32>(width, height);

            for (int i = 0; i < colors.Count; i++) {
                Rgba32 color = colors[i].ToImageSharpColor();
                int colorX = (i % ColorsPerRow) * ZoomSize;
                int colorY = (i / ColorsPerRow) * ZoomSize;

                // Repeat the same color to create a big square
                for (int zoomX = 0; zoomX < ZoomSize; zoomX++) {
                    for (int zoomY = 0; zoomY < ZoomSize; zoomY++) {
                        image[colorX + zoomX, colorY + zoomY] = color;
                    }
                }
            }

            var binary = new BinaryFormat();
            image.Save(binary.Stream, encoder);
            return binary;
        }
    }
}
