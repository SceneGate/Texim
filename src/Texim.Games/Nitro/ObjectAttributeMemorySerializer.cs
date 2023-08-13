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

    public static ushort[] ToBinary(ObjectAttributeMemory oam)
    {
        ushort attr0 = 0, attr1 = 0, attr2 = 0;

        (int shape, int sizeMode) = GetShape((oam.Width, oam.Height));

        // Attribute 0
        attr0 |= (ushort)(oam.CoordinateY & 0x0FF);
        attr0 |= (ushort)((oam.HasRotationOrScaling ? 1 : 0) << 8);
        if (oam.HasRotationOrScaling) {
            attr0 |= (ushort)((oam.HasDoubleSize ? 1 : 0) << 9);
        } else {
            attr0 |= (ushort)((oam.IsDisabled ? 1 : 0) << 9);
        }

        attr0 |= (ushort)((byte)oam.Mode << 10);
        attr0 |= (ushort)((oam.IsMosaic ? 1 : 0) << 12);
        attr0 |= (ushort)(((byte)oam.PaletteMode & 0x01) << 13);
        attr0 |= (ushort)((shape & 0x03) << 14);

        // Attribute 1
        int coordX = oam.CoordinateX;
        if (coordX < 0) {
            coordX = ((oam.CoordinateX * -1) - 1) ^ 0x1FF; // 9-bit negation + 1 (two's complement)
        }

        attr1 |= (ushort)(coordX & 0x1FF);

        if (oam.HasRotationOrScaling) {
            attr1 |= (ushort)((oam.RotationOrScalingGroup & 0x1F) << 9);
        } else {
            // Bits 9-11 not used
            attr1 |= (ushort)((oam.HorizontalFlip ? 1 : 0) << 12);
            attr1 |= (ushort)((oam.VerticalFlip ? 1 : 0) << 13);
        }

        attr1 |= (ushort)((sizeMode & 0x03) << 14);

        // Attribute 2
        int tileIndex = oam.TileIndex;
        if (oam.PaletteMode == NitroPaletteMode.Palette16x16) {
            tileIndex /= 2;
        }

        attr2 |= (ushort)(tileIndex & 0x3FF);
        attr2 |= (ushort)((oam.Layer & 0x03) << 10);
        attr2 |= (ushort)((oam.PaletteIndex & 0x0F) << 12);

        return new[] { attr0, attr1, attr2 };
    }

    public static ObjectAttributeMemory ReadOam(this DataReader reader) =>
        FromBinary(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());

    public static void WriteOam(this DataWriter writer, ObjectAttributeMemory oam)
    {
        ushort[] binary = ToBinary(oam);
        writer.Write(binary[0]);
        writer.Write(binary[1]);
        writer.Write(binary[2]);
    }

    public static ObjectAttributeMemory[] ReadOams(this DataReader reader, int numOams)
    {
        var oams = new ObjectAttributeMemory[numOams];
        for (int i = 0; i < numOams; i++) {
            oams[i] = reader.ReadOam();
        }

        return oams;
    }

    private static (int Shape, int SizeMode) GetShape((int Width, int Height) size)
    {
        int shape, sizeMode;
        if (size.Width == size.Height) {
            shape = 0;
            sizeMode = size.Width switch {
                8 => 0,
                16 => 1,
                32 => 2,
                64 => 3,
                _ => throw new ArgumentException(
                    "Invalid cell square size. It must be 8, 16, 32 or 64.",
                    nameof(size)),
            };
        } else if (size.Width > size.Height) {
            shape = 1;
            sizeMode = size switch {
                (Width: 16, Height: 8) => 0,
                (Width: 32, Height: 8) => 1,
                (Width: 32, Height: 16) => 2,
                (Width: 64, Height: 32) => 3,
                _ => throw new ArgumentException(
                    "Invalid cell horizontal size. It must be 16x8, 32x8, 32x16 or 64x32",
                    nameof(size)),
            };
        } else {
            shape = 2;
            sizeMode = size switch {
                (Width: 8, Height: 16) => 0,
                (Width: 8, Height: 32) => 1,
                (Width: 16, Height: 32) => 2,
                (Width: 32, Height: 64) => 3,
                _ => throw new ArgumentException(
                    "Invalid cell vertical size. It must be 8x16, 8x32, 16x32 or 32x64",
                    nameof(size)),
            };
        }

        return (shape, sizeMode);
    }
}
