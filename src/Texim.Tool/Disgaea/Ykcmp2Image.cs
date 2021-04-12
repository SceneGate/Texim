// Copyright (c) 2019 Benito Palacios Sanchez
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace Texim.Tool.Disgaea
{
    using System;
    using Texim.Colors;
    using Texim.Images;
    using Texim.Palettes;
    using Texim.Pixels;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public class Ykcmp2Image : IConverter<IBinary, FullImage>
    {
        private const int NumColors = 256;
        private const int Width = 256;
        private const int Height = 128;

        public FullImage Convert(IBinary source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            source.Stream.Position = 0;
            var reader = new DataReader(source.Stream);

            Rgb[] colors = reader.ReadColors<Rgba32>(NumColors);
            var palette = new Palette(colors);

            IndexedPixel[] pixels = reader.ReadPixels<Indexed8Bpp>(Width * Height);
            var indexed = new IndexedImage(Width, Height, pixels);

            return indexed.CreateFullImage(palette);
        }
    }
}
