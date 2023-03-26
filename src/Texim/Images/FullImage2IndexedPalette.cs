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
    using Texim.Palettes;
    using Texim.Pixels;
    using Texim.Processing;
    using Yarhl.FileFormat;

    public class FullImage2IndexedPalette :
        IInitializer<IQuantization>, IConverter<IFullImage, IndexedPaletteImage>
    {
        private IQuantization quantization;

        public void Initialize(IQuantization parameters)
        {
            quantization = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public IndexedPaletteImage Convert(IFullImage source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = quantization.Quantize(source.Pixels);

            var indexed = new IndexedPaletteImage {
                Width = source.Width,
                Height = source.Height,
                Pixels = result.Pixels,
            };
            indexed.Palettes.Add(result.Palettes.Palettes);

            return indexed;
        }
    }
}
