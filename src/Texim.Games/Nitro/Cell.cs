// Copyright (c) 2022 SceneGate

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

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sprites;

/// <summary>
/// Nitro sprite definition.
/// </summary>
public class Cell : ISprite
{
    public Cell()
    {
    }

    public Cell(Cell other, IEnumerable<IImageSegment> segments)
    {
        Segments = new Collection<IImageSegment>();
        foreach (IImageSegment segment in segments) {
            Segments.Add(segment is ObjectAttributeMemory ? segment : new ObjectAttributeMemory(segment));
        }

        Width = other.Width;
        Height = other.Height;
        Attributes = other.Attributes;
        BoundaryXStart = other.BoundaryXStart;
        BoundaryXEnd = other.BoundaryXEnd;
        BoundaryYStart = other.BoundaryYStart;
        BoundaryXEnd = other.BoundaryXEnd;
        UserExtendedCellAttribute = other.UserExtendedCellAttribute;
    }

    public Collection<IImageSegment> Segments { get; init; } = new Collection<IImageSegment>();

    public int Width { get; set; }

    public int Height { get; set; }

    public CellAttributes Attributes { get; set; }

    public int BoundaryXStart { get; set; }

    public int BoundaryXEnd { get; set; }

    public int BoundaryYStart { get; set; }

    public int BoundaryYEnd { get; set; }

    public uint UserExtendedCellAttribute { get; set; }
}
