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
namespace Texim.Games.Nitro;

using Texim.Images;
using Texim.Sprites;

public record FullImage2ReferenceNitroCellParams : FullImage2SpriteParams
{
    /// <summary>
    /// Gets the file path (without extension) to write the original
    /// and new images of layers that don't re-use the original image pixels
    /// to debug the algorithm.
    /// </summary>
    public string DebugNewLayersPath { get; init; }

    /// <summary>
    /// Gets a value indicating whether to prioritize new pixels in the
    /// top layers (e.g. text over clean button) or put then in
    /// the bottom layers (e.g. top layer is yellow border with transparent body).
    /// </summary>
    public bool ImportTopToBottom { get; init; } = true;

    /// <summary>
    /// Gets the cell to use to copy the metadata into the new.
    /// </summary>
    public Cell ReferenceCell { get; init; }

    /// <summary>
    /// Gets a value indicating whether the cell image has the format 8 bits per pixel
    /// or 4 bits per pixel.
    /// </summary>
    public bool Has8bppDepth { get; init; }

    /// <summary>
    /// Gets the image to update in the sprite.
    /// </summary>
    public IIndexedImage Image { get; init; }
}
