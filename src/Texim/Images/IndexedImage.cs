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
    using Texim.Formats;
    using Texim.Palettes;
    using Texim.Pixels;

    public class IndexedImage : IIndexedImage
    {
        private static Indexed2FullImage fullImageConverter = new Indexed2FullImage();

        public IndexedImage()
        {
        }

        public IndexedImage(int width, int height)
        {
            Pixels = new IndexedPixel[width * height];
            Width = width;
            Height = height;
        }

        public IndexedImage(int width, int height, IndexedPixel[] pixels)
        {
            Pixels = pixels;
            Width = width;
            Height = height;
        }

        public int Width { get; init; }

        public int Height { get; init; }

        public IndexedPixel[] Pixels { get; init; }

        public FullImage CreateFullImage(IPalette palette)
        {
            fullImageConverter.Initialize(palette);
            return fullImageConverter.Convert(this);
        }

        public FullImage CreateFullImage(IPaletteCollection palettes)
        {
            fullImageConverter.Initialize(palettes);
            return fullImageConverter.Convert(this);
        }

        public FullImage CreateFullImage(IPaletteCollection palettes, bool firstColorAsTransparent)
        {
            fullImageConverter.Initialize((palettes, firstColorAsTransparent));
            return fullImageConverter.Convert(this);
        }
    }
}
