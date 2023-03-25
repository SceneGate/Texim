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

using System;

public class CellAttributes
{
    public bool UseFlipFlags => throw new NotSupportedException("Unknown flag");

    public int RenderingOptimizationFlags => throw new NotSupportedException("Unknown flags");

    public int UnknownFlags { get; set; }

    public bool IsSquare { get; set; }

    public int SquareDimension { get; set; }

    public static CellAttributes FromBinary(ushort data) =>
        new CellAttributes {
            IsSquare = ((data >> 11) & 1) == 0,
            SquareDimension = (data & 0x3F) * 8,
            UnknownFlags = data >> 6,
        };

    public ushort ToBinary() =>
        (ushort)(((SquareDimension / 8) & 0x3F) |
        ((UnknownFlags & 0x1F) << 6) |
        (IsSquare ? 0 : 1) << 11);
}
