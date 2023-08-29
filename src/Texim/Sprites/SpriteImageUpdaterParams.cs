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

using System.Collections.Generic;
using Texim.Images;
using Texim.Pixels;

public record SpriteImageUpdaterParams
{
    /// <summary>
    /// Gets the image to update in the sprite.
    /// </summary>
    public IIndexedImage Image { get; init; }

    /// <summary>
    /// Gets the sequence of tiles the converter will use to search and add
    /// new pixels if not found.
    /// </summary>
    public List<IndexedPixel> PixelSequences { get; init; }

    /// <summary>
    /// Gets the minimum amount of pixels a sprite segment should have.
    /// Usually this is the same as <see cref="PixelsPerIndex"/>, except in
    /// NCER formats where it multiplies by a block size parameter.
    /// </summary>
    public int MinimumPixelsPerSegment { get; init; }

    /// <summary>
    /// Gets the amount of pixels each segment tile index increments.
    /// Usually this is the tile size (64).
    /// </summary>
    public int PixelsPerIndex { get; init; }

    /// <summary>
    /// Gets a value indicating whether this sprite supports horizontal and
    /// vertical flipping.
    /// </summary>
    /// <remarks>
    /// Some games may not support this feature of the segments.
    /// </remarks>
    public bool SupportsFlipping { get; init; }
}
