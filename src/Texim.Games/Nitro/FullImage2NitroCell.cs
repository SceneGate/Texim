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

using System;
using System.Linq;
using Texim.Images;
using Texim.Sprites;
using Yarhl.FileFormat;

public class FullImage2NitroCell :
    FullImage2Sprite,
    IInitializer<FullImage2NitroCellParams>
{
    private FullImage2NitroCellParams parameters;

    public void Initialize(FullImage2NitroCellParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        this.parameters = parameters;
        base.Initialize(parameters);
    }

    public override ISprite Convert(FullImage source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(parameters);

        var newSprite = base.Convert(source);

        // Add the metadata from the original sprite.
        // and convert the segments into OAMs.
        var newCell = new Cell(parameters.ReferenceCell, newSprite.Segments) {
            Width = newSprite.Width,
            Height = newSprite.Height,
        };

        // Add missing info specific to OAMs.
        foreach (var segment in newCell.Segments.OfType<ObjectAttributeMemory>()) {
            segment.PaletteMode = parameters.Has8bppDepth
                ? NitroPaletteMode.Palette256x1
                : NitroPaletteMode.Palette16x16;
        }

        return newCell;
    }
}