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
using Yarhl.FileSystem;
using Yarhl.IO;

/// <summary>
/// Converter that deserializes a NCER binary stream into a container of sprites.
/// </summary>
public class Binary2Ncer : NitroDeserializer<Ncer>
{
    protected override string Stamp => "NCER";

    protected override void ReadSection(DataReader reader, Ncer model, string id, int size)
    {
        switch (id) {
            case "CEBK":
                ReadCellBank(reader, model);
                break;

            case "LABL":
                model.Labels.Add(ReadLabelSection(reader));
                break;

            default:
                throw new FormatException($"Unknown section: {id}");
        }
    }

    private void ReadCellBank(DataReader reader, Ncer model)
    {
        long sectionPos = reader.Stream.Position;

        // Read header
        ushort numCells = reader.ReadUInt16();
        model.Attributes = (CellBankAttributes)reader.ReadUInt16();
        uint cellsDataOffset = reader.ReadUInt32();
        model.TileMapping = (CellTileMappingKind)reader.ReadInt32();
        _ = reader.ReadUInt32(); // VRAM transfer data info pointer. Unused.
        _ = reader.ReadUInt32(); // unused pointer
        uint extendedDataOffset = reader.ReadUInt32();

        int cellInfoSize = model.Attributes.HasFlag(CellBankAttributes.CellsWithBoundary) ? 0x10 : 0x08;
        long segmentsPosition = sectionPos + cellsDataOffset + (numCells * cellInfoSize);

        for (int i = 0; i < numCells; i++) {
            reader.Stream.Position = sectionPos + cellsDataOffset + (i * cellInfoSize);
            Cell cell = ReadCell(reader, model, segmentsPosition);
            model.Root.Add(new Node($"cell{i:D3}", cell));
        }

        if (extendedDataOffset != 0) {
            reader.Stream.Position = sectionPos + extendedDataOffset;
            model.HasCellExtendedInfo = true;
            FillUserExtendedCellAttributes(reader, model);
        }
    }

    private Cell ReadCell(DataReader reader, Ncer model, long segmentsPosition)
    {
        var cell = new Cell();

        ushort numSegments = reader.ReadUInt16();
        cell.Attributes = CellAttributes.FromBinary(reader.ReadUInt16());
        uint segmentsOffset = reader.ReadUInt32();

        if (cell.Attributes.IsSquare) {
            cell.Width = cell.Attributes.SquareDimension;
            cell.Height = cell.Attributes.SquareDimension;
        } else if (model.Attributes.HasFlag(CellBankAttributes.CellsWithBoundary)) {
            short maxX = reader.ReadInt16();
            short maxY = reader.ReadInt16();
            short minX = reader.ReadInt16();
            short minY = reader.ReadInt16();
            cell.Width = maxX - minX;
            cell.Height = maxY - minY;
            cell.BoundaryXStart = minX;
            cell.BoundaryXEnd = maxX;
            cell.BoundaryYStart = minY;
            cell.BoundaryYEnd = maxY;
        } else {
            cell.Width = 512;
            cell.Height = 256;
        }

        cell.Width = 512;
        cell.Height = 256;

        int tileBlockSize = model.TileMapping.GetTileBlockSize();
        reader.Stream.Position = segmentsPosition + segmentsOffset;
        for (int i = 0; i < numSegments; i++) {
            ObjectAttributeMemory oam = reader.ReadOam();
            oam.TileIndex *= tileBlockSize;
            cell.Segments.Add(oam);
        }

        return cell;
    }

    private void FillUserExtendedCellAttributes(DataReader reader, Ncer model)
    {
        if (reader.ReadString(4) != "TACU") {
            throw new FormatException("Invalid user extended cell section");
        }

        _ = reader.ReadUInt32(); // section size

        long sectionPos = reader.Stream.Position;
        ushort numCells = reader.ReadUInt16();
        if (model.Root.Children.Count != numCells) {
            throw new FormatException("Extension cells doesn't match number of cells");
        }

        ushort attributesPerCell = reader.ReadUInt16(); // must be 1
        if (attributesPerCell != 1) {
            throw new FormatException("Unknown cell extension section format");
        }

        uint pointerOffset = reader.ReadUInt32();
        for (int i = 0; i < numCells; i++) {
            reader.Stream.Position = sectionPos + pointerOffset + (i * 4);
            uint attributeOffset = reader.ReadUInt32();

            reader.Stream.Position = sectionPos + attributeOffset;
            model.Root.Children[i].GetFormatAs<Cell>()!
                .UserExtendedCellAttribute = reader.ReadUInt32();
        }
    }
}
