// Copyright (c) 2021 SceneGate

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
namespace Texim.PerformanceTest.ImageStructures
{
    using System;

    public readonly struct PixelRgb
    {
        public PixelRgb(byte index, byte alpha)
        {
            IsIndexed = true;
            Index = index;
            Alpha = alpha;
            Red = 0;
            Green = 0;
            Blue = 0;
        }

        public PixelRgb(byte red, byte green, byte blue, byte alpha)
        {
            IsIndexed = false;
            Index = 0;
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }

        public bool IsIndexed { get; init; }

        public byte Index { get; init; }

        public byte Red { get; init; }

        public byte Green { get; init; }

        public byte Blue { get; init; }

        public byte Alpha { get; init; }

        public readonly System.Drawing.Color ToColor()
        {
            if (!IsIndexed) {
                throw new FormatException("Pixel is indexed");
            }

            return System.Drawing.Color.FromArgb(Alpha, Red, Green, Blue);
        }
    }
}
