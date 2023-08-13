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

using System.IO;
using System.Linq;
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
        const uint dataOffset = 0x18; // after header
        long sectionPos = writer.Stream.Position;

        int numCells = model.Root.Children.Count;

        writer.Write((ushort)numCells);
        writer.Write((ushort)model.Attributes);
        writer.Write(dataOffset);
        writer.Write((uint)model.TileMapping);
        writer.Write(0x00); // VRAM transfer data info pointer
        writer.Write(0x00); // unused pointer
        writer.Write(0x00); // extended data offset

        int cellInfoSize = model.Attributes.HasFlag(CellBankAttributes.CellsWithBoundary) ? 0x10 : 0x08;
        long segmentsPosition = sectionPos + dataOffset + (numCells * cellInfoSize);

        // Pre-fill the cell data info, so it can write in the segments.
        writer.Stream.PushCurrentPosition();
        writer.WriteTimes(0x00, numCells * cellInfoSize);
        writer.Stream.PopPosition();

        Cell[] cells = model.Root.Children.Select(x => x.GetFormatAs<Cell>()).ToArray();
        for (int i = 0; i < cells.Length; i++) {
            writer.Stream.Position = sectionPos + dataOffset + (i * cellInfoSize);
            WriteCell(writer, model, cells[i], segmentsPosition);
        }

        if (model.HasCellExtendedInfo) {
            uint extendedOffset = (uint)(writer.Stream.Position - sectionPos);
            writer.Stream.PushToPosition(sectionPos + 0x14);
            writer.Write(extendedOffset);
            writer.Stream.PopPosition();

            WriteUserExtendedCellAttribute(writer, cells);
        }
    }

    private void WriteCell(DataWriter writer, Ncer model, Cell cell, long segmentsPosition)
    {
        long relativeDataOffset = writer.Stream.Length - segmentsPosition;
        writer.Write((ushort)cell.Segments.Count);
        writer.Write(cell.Attributes.ToBinary());
        writer.Write((uint)relativeDataOffset);

        if (model.Attributes.HasFlag(CellBankAttributes.CellsWithBoundary)) {
            writer.Write((short)cell.BoundaryXEnd);
            writer.Write((short)cell.BoundaryYEnd);
            writer.Write((short)cell.BoundaryXStart);
            writer.Write((short)cell.BoundaryYStart);
        }

        // again we assume at the end of the stream is were we write the OAMs
        _ = writer.Stream.Seek(0, SeekOrigin.End);
        int tileBlockSize = model.TileMapping.GetTileBlockSize();
        foreach (ObjectAttributeMemory oam in cell.Segments.Cast<ObjectAttributeMemory>()) {
            oam.TileIndex /= tileBlockSize; // temporary for serialization. This may cause issues in other threads
            writer.WriteOam(oam);
            oam.TileIndex *= tileBlockSize; // restore
        }
    }

    private void WriteUserExtendedCellAttribute(DataWriter writer, Cell[] cells)
    {
        const uint dataOffset = 0x08;
        long sectionPos = writer.Stream.Position;

        writer.Write("TACU", nullTerminator: false);
        writer.Write(0x00); // section size
        writer.Write((ushort)cells.Length);
        writer.Write((ushort)1); // attributes per cell
        writer.Write(dataOffset);

        // Offsets
        uint cellOffset = ((uint)cells.Length * sizeof(uint)) + dataOffset;
        for (int i = 0; i < cells.Length; i++) {
            writer.Write(cellOffset);
            cellOffset += sizeof(uint);
        }

        foreach (Cell cell in cells) {
            writer.Write(cell.UserExtendedCellAttribute);
        }

        uint sectionSize = (uint)(writer.Stream.Length - sectionPos);
        writer.Stream.Position = sectionPos + 4;
        writer.Write(sectionSize);
    }
}
