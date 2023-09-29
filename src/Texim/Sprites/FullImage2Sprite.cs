// Copyright (c) 2023 SceneGate

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
namespace Texim.Sprites;

using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Texim.Images;
using Texim.Pixels;
using Texim.Processing;
using Yarhl.FileFormat;

public class FullImage2Sprite :
    IInitializer<FullImage2SpriteParams>,
    IConverter<FullImage, ISprite>
{
    protected FullImage2SpriteParams Parameters { get; private set; }

    public void Initialize(FullImage2SpriteParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        Parameters = parameters;
    }

    public virtual ISprite Convert(FullImage source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(Parameters);

        // First segment the cell into smaller parts (OAMs).
        // FUTURE: Check if the image fits in the existing cells, if so, re-use
        // It's hard to segment properly our image as the original source image
        // would have proper size (not full canvas) and proper layers. If we can
        // re-use the original cells we will achieve better tile re-using.
        (Sprite newSprite, _) = Parameters.Segmentation.Segment(source);

        // Now over its segments (OAMs), let's update its tile index and
        // finding if there are matching sequences. Add them the new to the NCGR image.
        for (int i = 0; i < newSprite.Segments.Count; i++) {
            IImageSegment segment = newSprite.Segments[i];

            // Tiles are indexed pixels. We need to quantize first that section of the image.
            FullImage segmentImage = source.SubImage(segment, Parameters.RelativeCoordinates);

            // We simulate like the sub-image is a big tile for the quantization so it finds only one palette.
            // It will search for the best matching palette (e.g. cases of 16 palettes of 16 colors).
            // LIMITATION: this may find a different palette than the original (case of palettes having same colors)
            //    in that case it may not get the same indexes, so the compression will fail to find a matching tile.
            //    Recommendation is to order colors in palettes, so even if they find different indexes are same.
            // LIMITATION: We use the original palette, no new colors allowed.
            var quantization = new FixedPaletteTileQuantization(
                Parameters.Palettes,
                new Size(segment.Width, segment.Height),
                segment.Width);
            quantization.FirstAsTransparent = true;
            var quantizationResult = (quantization.Quantize(segmentImage.Pixels) as FixedPaletteTileQuantizationResult)!;

            // We had to swizzle. We can't compare lineal pixels as there isn't
            // a constant "image width" that generates a valid sequence of lineal
            // pixels. Each segment has a different width. That's why NCGR usually
            // saves 0xFFFF as width. Having 8 as false width is the same as swizzling
            // tileSize.Width == imageWidth -> nop operation (but we need a copy).
            IndexedPixel[] segmentTiles;
            if (Parameters.IsImageTiled) {
                var segmentSwizzler = new TileSwizzling<IndexedPixel>(segment.Width);
                segmentTiles = segmentSwizzler.Swizzle(quantizationResult.Pixels);
            } else {
                segmentTiles = quantizationResult.Pixels;
            }

            // Now find unique pixels and create new cell with metatada.
            var newSegment = AssignImageToSegment(segment, segmentTiles, quantizationResult.PaletteIndexes[0]);
            newSprite.Segments[i] = newSegment;
        }

        return newSprite;
    }

    protected virtual IImageSegment AssignImageToSegment(IImageSegment segmentStructure, IndexedPixel[] segmentTiles, byte paletteIndex)
    {
        var newSegment = new ImageSegment(segmentStructure);

        // We only support one palette per segment.
        newSegment.PaletteIndex = paletteIndex;

        int tileIndex;
        bool horizontalFlip = false;
        bool verticalFlip = false;

        var existingTiles = CollectionsMarshal.AsSpan(Parameters.PixelSequences);
        if (Parameters.SupportsFlipping) {
            var segmentSize = new Size(segmentStructure.Width, segmentStructure.Height);
            (tileIndex, horizontalFlip, verticalFlip) = PixelSequenceFinder.SearchFlipping(
                existingTiles,
                segmentTiles,
                Parameters.MinimumPixelsPerSegment,
                segmentSize,
                Parameters.Palettes.Palettes[paletteIndex]);
        } else {
            tileIndex = PixelSequenceFinder.Search(
                existingTiles,
                segmentTiles,
                Parameters.MinimumPixelsPerSegment,
                Parameters.Palettes.Palettes[paletteIndex]);
        }

        if (tileIndex == -1) {
            if (segmentTiles.Length < Parameters.MinimumPixelsPerSegment) {
                int paddingPixelNum = Parameters.MinimumPixelsPerSegment - segmentTiles.Length;
                segmentTiles = segmentTiles.Concat(new IndexedPixel[paddingPixelNum]).ToArray();
            }

            // Add sequence to the pixels.
            newSegment.TileIndex = Parameters.PixelSequences.Count / Parameters.PixelsPerIndex;
            Parameters.PixelSequences.AddRange(segmentTiles);
        } else {
            newSegment.TileIndex = tileIndex / Parameters.PixelsPerIndex;
        }

        newSegment.HorizontalFlip = horizontalFlip;
        newSegment.VerticalFlip = verticalFlip;

        return newSegment;
    }
}
