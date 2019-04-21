//
// ImageMapConverter.cs
//
// Author:
//       Priverop <Github!>
//
// Copyright (c) 2019 
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


namespace Texim
{
    using System;
    using System.Drawing;
    using Yarhl.FileFormat;
    using Texim.Processing;
    public class ImageMapConverter : IConverter<Bitmap, Tuple<Palette, PixelArray, MapInfo[]>>
    {
        public ImageMapConverter()
        {
            // Default parameters:
            // + Image will be HorizontalTiled
            // + Depth will be 8bpp (256/1).
            // + Transparent color will be magenta: (R:248, G:0, B:248)
            Format = ColorFormat.Indexed_8bpp;
            TileSize = new Size(8, 8);
            PixelEncoding = PixelEncoding.HorizontalTiles;
            Quantization = new BasicQuantization();
        }

        public ColorQuantization Quantization
        {
            get;
            set;
        }

        public ColorFormat Format
        {
            get;
            set;
        }

        public PixelEncoding PixelEncoding
        {
            get;
            set;
        }

        public Size TileSize
        {
            get;
            set;
        }

        public BgMode BgMode
        {
            get;
            set;
        }

        public Tuple<Palette, PixelArray, MapInfo[]> Convert(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            int width = bitmap.Width;
            int height = bitmap.Height;
            int maxColors = Format.MaxColors();

            // Quantizate image -> get pixels and palette
            Quantization.Quantizate(bitmap);
            Pixel[] pixels = Quantization.GetPixels(PixelEncoding);
            Color[] colors = Quantization.Palette;
            if (colors.Length > maxColors)
                throw new FormatException($"The image has more than {maxColors} colors");

            // Create palette format
            Palette palette = new Palette(colors);

            // Create map from pixels
            Map map = new Map()
            {
                TileSize = TileSize,
                Width = width,
                Height = height,
                BgMode = BgMode
            };



            pixels = map.CreateMap(pixels);

            // Create image format
            PixelArray image = new PixelArray
            {
                Width = (pixels.Length > 256) ? 256 : pixels.Length
            };

            image.Height = (int)Math.Ceiling(pixels.Length / (double)image.Width);
            if (image.Height % TileSize.Height != 0)
                image.Height += TileSize.Height - (image.Height % TileSize.Height);
            image.SetData(pixels, PixelEncoding, Format, TileSize);

            return new Tuple<Palette, PixelArray, MapInfo[]>(palette, image, map.GetMapInfo());
        }
    }
}
