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
namespace Texim.Tool.BlackRockShooter
{
    using System;
    using System.Drawing;
    using Texim.Colors;
    using Texim.Images;
    using Texim.Palettes;
    using Texim.Pixels;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public class Ptmd2Image : IConverter<IBinary, FullImage>
    {
        private const string Stamp = "PTMD";
        private const int Width = 256;
        private const int Height = 64;
        private static readonly Size TileSize = new Size(32, 8);

        public FullImage Convert(IBinary source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            source.Stream.Position = 0;
            DataReader reader = new DataReader(source.Stream);
            string stamp = reader.ReadString(4);
            if (stamp != Stamp) {
                throw new FormatException($"Invalid stamp: {stamp}");
            }

            reader.Stream.Position += 0xC; // unknown

            var tileSwizzling = new TileSwizzling<IndexedPixel>(TileSize, Width);
            var pixels = reader.ReadPixels<Indexed4bpp>(Width * Height)
                .UnswizzleWith(tileSwizzling);

            var image = new IndexedImage {
                Width = Width,
                Height = Height,
                Pixels = pixels,
            };

            // It isn't the palette but it could work for now
            // Are they coordinates?
            reader.Stream.Position = 0x2010;
            var palette = new Palette(reader.ReadColors<Bgr555>(16));

            return image.CreateFullImage(palette);
        }
    }
}
