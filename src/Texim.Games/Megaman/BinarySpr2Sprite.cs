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
namespace Texim.Games.Megaman;

using System;
using Texim.Colors;
using Texim.Images;
using Texim.Palettes;
using Texim.Pixels;
using Texim.Sprites;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

public class BinarySpr2Sprite : IConverter<IBinary, NodeContainerFormat>
{
    private DataReader reader;
    private uint dataOffset;
    private uint paletteOffset;
    private uint animationOffset;
    private uint spritesOffset;
    private int sceneWidth;
    private int sceneHeight;

    public NodeContainerFormat Convert(IBinary source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        var container = new NodeContainerFormat();
        reader = new DataReader(source.Stream);

        ReadHeader();

        var pixels = ReadPixels();
        container.Root.Add(new Node("pixels", pixels));

        var palettes = ReadPalettes();
        container.Root.Add(new Node("palettes", palettes));

        var animations = ReadAnimations();
        container.Root.Add(animations);

        return container;
    }

    private void ReadHeader()
    {
        dataOffset = reader.ReadUInt32();
        paletteOffset = reader.ReadUInt32();
        animationOffset = reader.ReadUInt32();
        spritesOffset = reader.ReadUInt32();
        reader.ReadUInt32(); // unknown, is tiled?
    }

    private IndexedImage ReadPixels()
    {
        reader.Stream.Position = dataOffset;
        sceneWidth = reader.ReadInt16();
        sceneHeight = reader.ReadInt16();
        uint pixelsOffset = reader.ReadUInt32(); // then there is an unknown uint per element

        reader.Stream.Position = dataOffset + pixelsOffset;
        int pixelsLength = (int)(paletteOffset - reader.Stream.Position);
        var pixels = reader.ReadPixels<Indexed4Bpp>(pixelsLength * 2)
            .UnswizzleWith(new TileSwizzling<IndexedPixel>(64));

        // sceneWidth happens to be also the num of tiles, so we can use it as height and 64 as width
        return new IndexedImage {
            Pixels = pixels,
            Width = 64,
            Height = sceneHeight,
        };
    }

    private PaletteCollection ReadPalettes()
    {
        reader.Stream.Position = paletteOffset;

        reader.ReadInt16(); // color format?
        short numPalettes = reader.ReadInt16();

        var palettes = new PaletteCollection();
        for (int i = 0; i < numPalettes; i++) {
            var palette = new Palette(reader.ReadColors<Bgr555>(16));
            palettes.Palettes.Add(palette);
        }

        return palettes;
    }

    private Node ReadAnimations()
    {
        var animationNode = new Node("animations");

        reader.Stream.Position = animationOffset;
        int numAnimations = reader.ReadInt32();
        for (int i = 0; i < numAnimations; i++) {
            var node = ReadAnimationSprites(i);
            animationNode.Add(node);
        }

        return animationNode;
    }

    private Node ReadAnimationSprites(int index)
    {
        reader.Stream.Position = animationOffset;
        int numAnimations = reader.ReadInt32();

        reader.Stream.Position += index * 4;
        uint offset = reader.ReadUInt32();
        uint nextOffset = (index == numAnimations - 1)
            ? (spritesOffset - animationOffset)
            : reader.ReadUInt32();
        int numSprites = (int)(nextOffset - offset) / 4;

        var spritesNode = new Node($"ani_{index}");
        reader.Stream.Position = animationOffset + offset;
        for (int i = 0; i < numSprites; i++) {
            int spriteIndex = reader.ReadByte();
            reader.ReadByte(); // unknown, animation related?
            reader.ReadByte(); // unknown, animation related?
            byte paletteIndex = reader.ReadByte();

            reader.Stream.PushCurrentPosition();
            var sprite = ReadSprite(spriteIndex, paletteIndex);
            spritesNode.Add(new Node($"frame_{i}", sprite));
            reader.Stream.PopPosition();
        }

        return spritesNode;
    }

    private Sprite ReadSprite(int index, byte paletteIndex)
    {
        reader.Stream.Position = spritesOffset;
        int numSprites = reader.ReadInt32();

        reader.Stream.Position += index * 4;
        uint offset = reader.ReadUInt32();
        uint nextOffset = (index == numSprites - 1)
            ? (uint)(reader.Stream.Length - spritesOffset)
            : reader.ReadUInt32();
        int numElements = (int)(nextOffset - offset) / 8;

        var sprite = new Sprite {
            Width = sceneWidth,
            Height = sceneHeight,
        };

        reader.Stream.Position = spritesOffset + offset;
        for (int i = 0; i < numElements; i++) {
            int tileIndex = reader.ReadByte() * 2; // x2 because of 4bpp
            int coordX = reader.ReadSByte();
            int coordY = reader.ReadSByte();
            int sizeMode = reader.ReadByte();
            int shape = reader.ReadByte();
            reader.ReadByte(); // unknown
            int layer = reader.ReadByte();
            reader.ReadByte(); // unknown

            var (width, height) = GetSize(shape, sizeMode);

            sprite.Segments.Add(new ImageSegment {
                TileIndex = tileIndex,
                CoordinateX = coordX,
                CoordinateY = coordY,
                Width = width,
                Height = height,
                Layer = layer,
                PaletteIndex = paletteIndex,
                HorizontalFlip = false,
                VerticalFlip = false,
            });
        }

        return sprite;
    }

    private (int width, int height) GetSize(int shape, int mode)
    {
        int[,] sizeMatrix = new int[3, 4] {
            { 8, 16, 32, 64 }, // Square
            { 16, 32, 32, 64 }, // Horizontal
            { 8, 8, 16, 32 }, // Vertical
        };

        return shape switch {
            0 => (sizeMatrix[0, mode], sizeMatrix[0, mode]),
            1 => (sizeMatrix[1, mode], sizeMatrix[2, mode]),
            2 => (sizeMatrix[2, mode], sizeMatrix[1, mode]),
            _ => throw new FormatException("Unknown size mode")
        };
    }
}
