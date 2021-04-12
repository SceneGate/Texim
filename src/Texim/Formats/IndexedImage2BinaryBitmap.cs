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
    using System.Drawing;
    using System.Linq;
    using Texim.Images;
    using Texim.Palettes;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public class IndexedImage2BinaryBitmap :
        IInitializer<IndexedImageBitmapParameters>,
        IConverter<IIndexedImage, BinaryFormat>
    {
        private IndexedImageBitmapParameters parameters;

        public void Initialize(IndexedImageBitmapParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            this.parameters = parameters;
        }

        public BinaryFormat Convert(IIndexedImage source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // Preconvert to colors for faster iteration later
            Color[] PaletteToColors(IPalette pal) =>
                pal.Colors.Select(c => c.ToColor()).ToArray();

            Color[][] palettes = (parameters.Palettes != null)
                ? parameters.Palettes.Palettes.Select(PaletteToColors).ToArray()
                : new[] { PaletteToColors(parameters.Palette) };

            var bitmap = new Bitmap(source.Width, source.Height);
            for (int x = 0; x < source.Width; x++) {
                for (int y = 0; y < source.Height; y++) {
                    var pixel = source.Pixels[y * source.Width + x];
                    if (pixel.PaletteIndex >= palettes.Length) {
                        throw new FormatException("Missing palettes");
                    }

                    var palette = palettes[pixel.PaletteIndex];
                    if (pixel.Index >= palette.Length) {
                        throw new FormatException("Missing colors in palette");
                    }

                    Color color = (pixel.Alpha != 255)
                        ? Color.FromArgb(pixel.Alpha, palette[pixel.Index])
                        : palette[pixel.Index];
                    bitmap.SetPixel(x, y, color);
                }
            }

            var binary = new BinaryFormat();
            bitmap.Save(binary.Stream, parameters.Format);
            return binary;
        }
    }
}
