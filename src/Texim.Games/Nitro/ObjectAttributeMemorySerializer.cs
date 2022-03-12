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

public static class ObjectAttributeMemorySerializer
{
    private static readonly int[,] SizeMatrix = new int[,] {
        { 8, 16, 32, 64 }, // Square
        { 16, 32, 32, 64 }, // Horizontal
        { 8, 8, 16, 32 }, // Vertical
    };

    public static ObjectAttributeMemory FromBinary(ushort attr0, ushort attr1, ushort attr2)
    {
        var oam = new ObjectAttributeMemory();

        // Attribute 0
        oam.CoordinateY = (sbyte)(attr0 & 0x0FF);
        oam.HasRotationOrScaling = ((attr0 >> 8) & 0x01) == 1;
        if (oam.HasRotationOrScaling) {
            oam.HasDoubleSize = ((attr0 >> 9) & 0x01) == 1;
        } else {
            oam.IsDisabled = ((attr0 >> 9) & 0x01) == 1;
        }

        oam.Mode = (ObjectAttributeMemoryMode)((attr0 >> 10) & 0x03);
        oam.IsMosaic = ((attr0 >> 12) & 0x01) == 1;
        oam.PaletteMode = (NitroPaletteMode)((attr0 >> 13) & 0x01);
        int shape = (attr0 >> 14) & 0x03;

        // Attribute 1
        int coordX = attr1 & 0x1FF;
        if (coordX >> 8 == 0) {
            oam.CoordinateX = coordX;
        } else {
            oam.CoordinateX = ((coordX ^ 0x1FF) + 1) * -1; // 9-bit negation + 1 (two's complement)
        }

        if (oam.HasRotationOrScaling) {
            oam.RotationOrScalingGroup = (byte)((attr1 >> 9) & 0x1F);
        } else {
            // Bits 9-11 not used
            oam.HorizontalFlip = ((attr1 >> 12) & 0x01) == 1;
            oam.VerticalFlip = ((attr1 >> 13) & 0x01) == 1;
        }

        int sizeMode = (attr1 >> 14) & 0x03;

        // Attribute 2
        oam.TileIndex = (short)(attr2 & 0x3FF);
        if (oam.PaletteMode == NitroPaletteMode.Palette16x16) {
            oam.TileIndex *= 2;
        }

        oam.Layer = (attr2 >> 10) & 0x3;
        oam.PaletteIndex = (byte)((attr2 >> 12) & 0x0F);

        if (shape == 0) {
            // Square
            oam.Width = SizeMatrix[0, sizeMode];
            oam.Height = SizeMatrix[0, sizeMode];
        } else if (shape == 1) {
            // Horizontal
            oam.Width = SizeMatrix[1, sizeMode];
            oam.Height = SizeMatrix[2, sizeMode];
        } else if (shape == 2) {
            // Vertical
            oam.Width = SizeMatrix[2, sizeMode];
            oam.Height = SizeMatrix[1, sizeMode];
        }

        return oam;
    }

    public static ObjectAttributeMemory ReadOam(this DataReader reader) =>
        FromBinary(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());

    public static ObjectAttributeMemory[] ReadOams(this DataReader reader, int numOams)
    {
        var oams = new ObjectAttributeMemory[numOams];
        for (int i = 0; i < numOams; i++) {
            oams[i] = reader.ReadOam();
        }

        return oams;
    }
}
