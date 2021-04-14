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
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Texim.Images;
    using Texim.Pixels;
    using Yarhl.FileFormat;

    public class MapCompression :
        IInitializer<Size>, IConverter<IIndexedImage, IndexedMapImage>
    {
        private Size tileSize = new Size(8, 8);

        public void Initialize(Size parameters) => tileSize = parameters;

        public IndexedMapImage Convert(IIndexedImage source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // First we need to do tile swizzling
            var swizzling = new TileSwizzling<IndexedPixel>(source.Width);
            var tiles = swizzling.Swizzle(source.Pixels).AsMemory();

            int pixelsPerTile = tileSize.Width * tileSize.Height;
            int numTiles = tiles.Length / pixelsPerTile;
            var uniqueTiles = new List<Memory<IndexedPixel>>();
            var maps = new MapInfo[numTiles];

            for (int i = 0; i < numTiles; i++) {
                var tile = tiles.Slice(i * pixelsPerTile, pixelsPerTile);

                int repeatedIdx = uniqueTiles.FindIndex(t => t.HasEquivalentIndexes(tile));
                if (repeatedIdx != -1) {
                    maps[i] = new MapInfo {
                        TileIndex = (short)repeatedIdx,
                        HorizontalFlip = false,
                        VerticalFlip = false,
                        PaletteIndex = tile.Span[0].PaletteIndex,
                    };
                    continue;
                }

                tile.FlipHorizontal(tileSize);
                repeatedIdx = uniqueTiles.FindIndex(t => t.HasEquivalentIndexes(tile));
                if (repeatedIdx != -1) {
                    maps[i] = new MapInfo {
                        TileIndex = (short)repeatedIdx,
                        HorizontalFlip = true,
                        VerticalFlip = false,
                        PaletteIndex = tile.Span[0].PaletteIndex,
                    };
                    continue;
                }

                tile.FlipVertical(tileSize);
                repeatedIdx = uniqueTiles.FindIndex(t => t.HasEquivalentIndexes(tile));
                if (repeatedIdx != -1) {
                    maps[i] = new MapInfo {
                        TileIndex = (short)repeatedIdx,
                        HorizontalFlip = true,
                        VerticalFlip = true,
                        PaletteIndex = tile.Span[0].PaletteIndex,
                    };
                    continue;
                }

                tile.FlipHorizontal(tileSize);
                repeatedIdx = uniqueTiles.FindIndex(t => t.HasEquivalentIndexes(tile));
                if (repeatedIdx != -1) {
                    maps[i] = new MapInfo {
                        TileIndex = (short)repeatedIdx,
                        HorizontalFlip = false,
                        VerticalFlip = true,
                        PaletteIndex = tile.Span[0].PaletteIndex,
                    };
                    continue;
                }

                tile.FlipVertical(tileSize);
                maps[i] = new MapInfo {
                    TileIndex = (short)uniqueTiles.Count,
                    HorizontalFlip = false,
                    VerticalFlip = false,
                    PaletteIndex = tile.Span[0].PaletteIndex,
                };
                uniqueTiles.Add(tile);
            }

            var uniquePixels = uniqueTiles.SelectMany(t => t.ToArray()).ToArray();
            uniquePixels = swizzling.Unswizzle(uniquePixels);

            return new IndexedMapImage {
                Width = source.Width,
                Height = source.Height,
                Maps = maps,
                Pixels = uniquePixels,
            };
        }
    }
}
