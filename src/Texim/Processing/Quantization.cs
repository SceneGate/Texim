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
namespace Texim.Processing
{
    using Texim.Colors;
    using Texim.Palettes;
    using Texim.Pixels;

    public class Quantization : IQuantization
    {
        public bool FirstAsTransparent { get; set; }

        public Rgb AlphaColor { get; set; }

        public QuantizationResult Quantize(Rgb[] pixels)
        {
            var indexed = new IndexedPixel[pixels.Length];
            var palette = new Palette();

            if (FirstAsTransparent) {
                palette.Colors.Add(AlphaColor);
            }

            for (int i = 0; i < pixels.Length; i++) {
                if (FirstAsTransparent && pixels[i].Alpha <= 128) {
                    indexed[i] = new IndexedPixel(0, pixels[i].Alpha, 0);
                    continue;
                }

                int colorIdx = palette.Colors.IndexOf(pixels[i]);
                if (colorIdx == -1) {
                    colorIdx = palette.Colors.Count;
                    palette.Colors.Add(pixels[i]);
                }

                indexed[i] = new IndexedPixel((short)colorIdx, pixels[i].Alpha, 0);
            }

            var collection = new PaletteCollection(palette);
            return new QuantizationResult {
                Pixels = indexed,
                Palettes = collection,
            };
        }
    }
}
