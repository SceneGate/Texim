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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texim.Images;
using Texim.Palettes;
using Texim.Sprites;

public class Sprite2TiffParams
{
    /// <summary>
    /// Gets or sets a value indicating whether to create a TIFF file with
    /// colormap (palette) or export as RGB colors.
    /// </summary>
    public bool ExportAsIndexedImage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to try to reduce the number of
    /// layers by grouping adjacent layers using the same palette.
    /// </summary>
    public bool ReduceLayers { get; set; }

    /// <summary>
    /// Gets or sets the palettes of the sprite images.
    /// </summary>
    public IPaletteCollection Palettes { get; set; }

    /// <summary>
    /// Gets or sets the relative coordinates of the sprite segments.
    /// </summary>
    /// <remarks>
    /// <see cref="SpriteRelativeCoordinatesKind.Reset"/> is not accepted.
    /// </remarks>
    public SpriteRelativeCoordinatesKind RelativeCoordinates { get; set; }

    /// <summary>
    /// Gets or sets the pixels for the sprite image.
    /// </summary>
    public IIndexedImage Image { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the pixels of the image are tiled
    /// and the sprite tile indexed references refer to tiles.
    /// </summary>
    public bool IsTiled { get; set; }

    /// <summary>
    /// Gets or sets the size of the tiles when the image is tiled.
    /// </summary>
    public Size TileSize { get; set; } = new Size(8, 8);
}
