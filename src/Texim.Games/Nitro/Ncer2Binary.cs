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

using Yarhl.IO;

public class Ncer2Binary : NitroSerializer<Ncer>
{
    public Ncer2Binary()
    {
        RegisterSection("CEBK", WriteCellBank);
        RegisterSection("LABL", (writer, model) => WriteLabelSection(writer, model.Labels));
    }

    protected override string Stamp => "NCER";

    private void WriteCellBank(DataWriter writer, Ncer model)
    {
        long sectionPos = writer.Stream.Position;

        writer.Write((ushort)model.Root.Children.Count);
        writer.Write((ushort)model.Attributes);
        writer.Write(0x00); // TODO: data offset
        writer.Write((uint)model.TileMapping);
        writer.Write(0x00); // VRAM transfer data info pointer
        writer.Write(0x00); // unused pointer
        writer.Write(0x00); // TODO: extended data offset


    }
}
