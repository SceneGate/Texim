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
namespace Texim.Disgaea
{
    using System;
    using Yarhl.FileFormat;
    using Yarhl.IO;
    using Media.Image;
    using System.Collections.Generic;
    using System.Drawing;

    public class Binary2Ykcmp :
        IConverter<BinaryFormat, Ykcmp>
    {
        public Ykcmp Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            DataReader reader = new DataReader(source.Stream);

            Ykcmp image = new Ykcmp();

            List<Color> palette = new List<Color>();
            const int PaletteSize = 0x400 / 4;
            for (int i = 0; i < PaletteSize; i++) {
                byte red = reader.ReadByte();
                byte green = reader.ReadByte();
                byte blue = reader.ReadByte();
                byte alpha = reader.ReadByte();
                palette.Add(Color.FromArgb(alpha, red, green, blue));
            }

            image.Palette = new Palette(palette.ToArray());

            image.Pixels = new PixelArray {
                Width = 256,
                Height = 128,
            };

            image.Pixels.SetData(
                reader.ReadBytes(0x8000),
                PixelEncoding.Lineal,
                ColorFormat.Indexed_8bpp);

            return image;
        }
    }
}
