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
namespace Texim.Tool.Raw
{
    using System;
    using Texim.Formats;
    using Texim.Pixels;

    public class PixelsConfiguration
    {
        public string Path { get; set; }

        public long Offset { get; set; }

        public int Size { get; set; }

        public string Encoding { get; set; }

        public string Swizzling { get; set; }

        public bool MissingBgTile { get; set; }

        public RawIndexedImageParams GetParams()
        {
            IIndexedPixelEncoding encoding = Encoding.ToUpperInvariant() switch {
                "8BPP" => Indexed8Bpp.Instance,
                "4BPP" => Indexed4Bpp.Instance,
                _ => throw new NotSupportedException("Unknown pixel encoding"),
            };

            ISwizzling<IndexedPixel> swizzling = null;
            if (Swizzling.StartsWith("TILE", StringComparison.InvariantCultureIgnoreCase)) {
                swizzling = new TileSwizzling<IndexedPixel>(8);
            }

            return new RawIndexedImageParams {
                Offset = Offset,
                Size = Size,
                PixelEncoding = encoding,
                Swizzling = swizzling,
            };
        }
    }
}
