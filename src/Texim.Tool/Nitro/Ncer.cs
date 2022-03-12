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
namespace Texim.Tool.Nitro;

using System;
using System.Collections.ObjectModel;
using Yarhl.FileSystem;

/// <summary>
/// Nitro CEll Resource. Format that contains sprite definitions.
/// </summary>
public class Ncer : NodeContainerFormat, INitroFormat
{
    public Ncer()
    {
        Version = new Version(1, 0);
    }

    public Version Version { get; set; }

    public CellBankAttributes Attributes { get; set; }

    public CellTileMappingKind TileMapping { get; set; }

    public byte[] UserExtendedInfo { get; set; }

    public Collection<string> Labels { get; init; } = new Collection<string>();
}
