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
namespace Texim.Compressions.Nitro
{
    using System;
    using System.Drawing;
    using Texim.Images;
    using Texim.Pixels;
    using Yarhl.FileFormat;

    public class MapDecompression :
        IInitializer<MapDecompressionParameters>,
        IConverter<IIndexedImage, IndexedImage>,
        IConverter<IndexedPixel[], IndexedPixel[]>
    {
        private Size tileSize;
        private IScreenMap map;

        public void Initialize(MapDecompressionParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            tileSize = parameters.TileSize;
            map = parameters.Map;
        }

        public IndexedImage Convert(IIndexedImage source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (map == null)
                throw new InvalidOperationException("Missing initialization");

            // It's easier if we swizzle again
            var swizzling = new TileSwizzling<IndexedPixel>(source.Width);
            IndexedPixel[] tiles = swizzling.Swizzle(source.Pixels);

            IndexedPixel[] decompressed = DecompressTiles(tiles);
            decompressed = swizzling.Unswizzle(decompressed);

            return new IndexedImage(map.Width, map.Height, decompressed);
        }

        public IndexedPixel[] Convert(IndexedPixel[] source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (map == null)
                throw new InvalidOperationException("Missing initialization");

            return DecompressTiles(source);
        }

        private IndexedPixel[] DecompressTiles(IndexedPixel[] source)
        {
            int pixelsPerTile = tileSize.Width * tileSize.Height;

            var decompressed = new IndexedPixel[map.Width * map.Height];
            Span<IndexedPixel> pixelsOut = decompressed;
            Span<IndexedPixel> pixelsIn = source;

            for (int i = 0; i < map.Maps.Length; i++) {
                MapInfo info = map.Maps[i];

                int inIndex = info.TileIndex * pixelsPerTile;
                Span<IndexedPixel> tileIn = pixelsIn.Slice(inIndex, pixelsPerTile);

                int outIndex = i * pixelsPerTile;
                Span<IndexedPixel> tileOut = pixelsOut.Slice(outIndex, pixelsPerTile);

                for (int t = 0; t < pixelsPerTile; t++) {
                    tileOut[t] = new IndexedPixel(
                        tileIn[t].Index,
                        tileIn[t].Alpha,
                        info.PaletteIndex);
                }

                if (info.HorizontalFlip) {
                    FlipHorizontal(tileOut);
                }

                if (info.VerticalFlip) {
                    FlipVertical(tileOut);
                }
            }

            return decompressed;
        }

        private void FlipHorizontal(Span<IndexedPixel> tile)
        {
            for (int y = 0; y < tileSize.Height; y++) {
                for (int x = 0; x < tileSize.Width / 2; x++) {
                    int t1 = (y * tileSize.Width) + x;
                    int t2 = (y * tileSize.Width) + (tileSize.Width - 1 - x);

                    IndexedPixel swap = tile[t1];
                    tile[t1] = tile[t2];
                    tile[t2] = swap;
                }
            }
        }

        private void FlipVertical(Span<IndexedPixel> tile)
        {
            for (int x = 0; x < tileSize.Width; x++) {
                for (int y = 0; y < tileSize.Height / 2; y++) {
                    int t1 = x + (tileSize.Width * y);
                    int t2 = x + (tileSize.Width * (tileSize.Height - 1 - y));

                    IndexedPixel swap = tile[t1];
                    tile[t1] = tile[t2];
                    tile[t2] = swap;
                }
            }
        }
    }
}
