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
namespace Texim.Sprites;

using System;
using System.Collections.ObjectModel;
using System.Drawing;
using Texim.Colors;
using Texim.Palettes;
using Texim.Pixels;

public static class PixelSequenceFinder
{
    private const int IndexesCount = 256;

    public static int Search(
        ReadOnlySpan<IndexedPixel> pixels,
        ReadOnlySpan<IndexedPixel> sequence,
        int blockSize,
        IPalette palette = null)
    {
        // It would be better to not have optional args
        if (palette?.Colors.Count > IndexesCount) {
            throw new ArgumentException($"Palettes with more than {IndexesCount} colors not supported");
        }

        bool[,] equivalentIndexes = palette is null ? null : InitializeEquivalentIndexes(palette.Colors);
        return Search(pixels, sequence, blockSize, equivalentIndexes);
    }

    public static (int TileIdx, bool HorizontalFlip, bool VerticalFlip) SearchFlipping(
        ReadOnlySpan<IndexedPixel> pixels,
        ReadOnlySpan<IndexedPixel> sequence,
        int blockSize,
        Size sequenceSize,
        IPalette palette = null)
    {
        if (palette?.Colors.Count > IndexesCount) {
            throw new ArgumentException($"Palettes with more than {IndexesCount} colors not supported");
        }

        bool[,] equivalentIndexes = palette is null ? null : InitializeEquivalentIndexes(palette.Colors);

        int tileIndex = Search(pixels, sequence, blockSize, equivalentIndexes);
        if (tileIndex != -1) {
            return (tileIndex, false, false);
        }

        // Create a copy so we don't modify the originals
        // anyway as we set the flag to flip, we shouldn't return/modify the pixels
        // we are going to write.
        var testSequence = sequence.ToArray().AsSpan();

        testSequence.FlipHorizontal(sequenceSize);
        tileIndex = Search(pixels, testSequence, blockSize, equivalentIndexes);
        if (tileIndex != -1) {
            return (tileIndex, true, false);
        }

        testSequence.FlipVertical(sequenceSize);
        tileIndex = Search(pixels, testSequence, blockSize, equivalentIndexes);
        if (tileIndex != -1) {
            return (tileIndex, true, true);
        }

        testSequence.FlipHorizontal(sequenceSize);
        tileIndex = Search(pixels, testSequence, blockSize, equivalentIndexes);
        if (tileIndex != -1) {
            return (tileIndex, false, true);
        }

        return (-1, false, false);
    }

    private static bool[,] InitializeEquivalentIndexes(Collection<Rgb> colors)
    {
        bool[,] equivalentIndexes = new bool[IndexesCount, IndexesCount];

        for (int i = 0; i < colors.Count; i++) {
            Rgb currentColor = colors[i];

            for (int j = 0; j < colors.Count; j++) {
                equivalentIndexes[i, j] = colors[j].Equals(currentColor);
            }
        }

        return equivalentIndexes;
    }

    private static int Search(
        ReadOnlySpan<IndexedPixel> pixels,
        ReadOnlySpan<IndexedPixel> sequence,
        int blockSize,
        bool[,] equivalentIndexes)
    {
        int foundPos = -1;

        for (int current = 0; current + blockSize <= pixels.Length && foundPos == -1; current += blockSize) {
            if (current + sequence.Length > pixels.Length) {
                break;
            }

            foundPos = current;
            if (HasSequence(pixels, sequence, current, equivalentIndexes)) {
                continue;
            }

            foundPos = -1;
        }

        return foundPos;
    }

    private static bool HasSequence(
        ReadOnlySpan<IndexedPixel> pixels,
        ReadOnlySpan<IndexedPixel> sequence,
        int startPos,
        bool[,] equivalentColorIndexes = null)
    {
        bool hasSequence = true;
        for (int i = 0; i < sequence.Length && hasSequence; i++) {
            IndexedPixel inputPixel = sequence[i];
            IndexedPixel exisitingPixel = pixels[startPos + i];

            bool hasSameIndex = inputPixel.Index == exisitingPixel.Index;
            bool hasEquivalentColor = hasSameIndex ||
                equivalentColorIndexes?[inputPixel.Index, exisitingPixel.Index] == true;

            hasSequence = hasSameIndex || hasEquivalentColor;
        }

        return hasSequence;
    }
}
