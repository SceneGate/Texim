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
namespace Texim.Formats;

using System;
using Texim.Images;
using Texim.Palettes;
using Texim.Sprites;
using Yarhl.FileFormat;

public class Tiff2Sprite : IConverter<TiffImage, ISprite>
{
    private readonly IConverter<FullImage, ISprite> layerConverter;
    private readonly SpriteRelativeCoordinatesKind targetCoordKind;

    public Tiff2Sprite(IConverter<FullImage, ISprite> layerConverter, SpriteRelativeCoordinatesKind targetCoordKind)
    {
        this.layerConverter = layerConverter ?? throw new ArgumentNullException(nameof(layerConverter));
        this.targetCoordKind = targetCoordKind;
    }

    public ISprite Convert(TiffImage source)
    {
        ArgumentNullException.ThrowIfNull(source);

        ISprite newSprite = null;

        // Repeat importer per page accumulating the image like combining images.
        foreach (TiffPage layer in source.Pages) {
            FullImage layerImage = GetRgbImage(layer);

            ISprite layerSprite = layerConverter.Convert(layerImage);

            // Take the first sprite generated from the converter
            // so we get the final type with any metadata embedded.
            if (newSprite is null) {
                newSprite = layerSprite;
            } else {
                newSprite.Segments.Add(layerSprite.Segments);
            }

            foreach (IImageSegment segment in layerSprite.Segments) {
                segment.CoordinateX += layer.X;
                segment.CoordinateY += layer.Y;

                if (targetCoordKind == SpriteRelativeCoordinatesKind.Center) {
                    segment.CoordinateX -= source.CanvasWidth / 2;
                    segment.CoordinateY -= source.CanvasHeight / 2;
                }
            }
        }

        return newSprite;
    }

    private static FullImage GetRgbImage(TiffPage page)
    {
        if (page.IsIndexed) {
            var layerIndexedImage = new IndexedPaletteImage {
                Height = page.Height,
                Width = page.Width,
                Pixels = page.IndexedPixels,
            };
            layerIndexedImage.Palettes.Add(new Palette(page.ColorMap));

            return layerIndexedImage.CreateFullImage();
        }

        return new FullImage(page.Width, page.Height) {
            Pixels = page.RgbPixels,
        };
    }
}
