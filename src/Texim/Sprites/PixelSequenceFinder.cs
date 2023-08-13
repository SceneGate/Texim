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
using Texim.Pixels;

public static class PixelSequenceFinder
{
    public static int Search(
        ReadOnlySpan<IndexedPixel> pixels,
        ReadOnlySpan<IndexedPixel> sequence,
        int blockSize)
    {
        int foundPos = -1;

        for (int current = 0; current + blockSize < pixels.Length && foundPos == -1; current += blockSize) {
            if (current + sequence.Length > pixels.Length) {
                break;
            }

            foundPos = current;
            if (HasSequence(pixels, sequence, current)) {
                continue;
            }

            // TODO: try again flipping the sequence.
            // In that case we may want to change the return type to a class.
            foundPos = -1;
        }

        return foundPos;
    }

    private static bool HasSequence(
        ReadOnlySpan<IndexedPixel> pixels,
        ReadOnlySpan<IndexedPixel> sequence,
        int startPos)
    {
        bool hasSequence = true;
        for (int i = 0; i < sequence.Length && hasSequence; i++) {
            if (pixels[startPos + i].Index != sequence[i].Index) {
                hasSequence = false;
            }
        }

        return hasSequence;
    }
}
