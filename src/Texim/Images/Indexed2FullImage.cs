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
namespace Texim.Images
{
    using System;
    using Texim.Colors;
    using Texim.Palettes;
    using Texim.Pixels;
    using Yarhl.FileFormat;

    public class Indexed2FullImage : IConverter<IIndexedImage, FullImage>
    {
        private readonly PaletteCollection palettes;

        public Indexed2FullImage(IPalette palette)
        {
            ArgumentNullException.ThrowIfNull(palette);
            palettes = new PaletteCollection(palette);
        }

        public Indexed2FullImage(IPaletteCollection parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);
            palettes = new PaletteCollection(parameters.Palettes);
        }

        public FullImage Convert(IIndexedImage source)
        {
            ArgumentNullException.ThrowIfNull(source);

            var image = new FullImage(source.Width, source.Height);
            for (int i = 0; i < source.Pixels.Length; i++) {
                IndexedPixel pixel = source.Pixels[i];
                if (pixel.PaletteIndex > palettes.Palettes.Count) {
                    throw new FormatException("Missing palettes");
                }

                var color = palettes.Palettes[pixel.PaletteIndex].Colors[pixel.Index];
                image.Pixels[i] = (pixel.Alpha == 255) ? color : new Rgb(color, pixel.Alpha);
            }

            return image;
        }
    }
}
