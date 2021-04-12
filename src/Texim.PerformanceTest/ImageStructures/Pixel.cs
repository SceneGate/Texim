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
    using System.Drawing;

    public readonly struct Pixel
    {
        private readonly int value;
        private readonly bool isIndexed;

        public Pixel(int index, byte alpha)
        {
            isIndexed = true;
            value = index | (alpha << 24);
        }

        public Pixel(byte red, byte green, byte blue, byte alpha)
        {
            isIndexed = false;
            value = red | (green << 8) | (blue << 16) | (alpha << 24);
        }

        public int Index { get => value & 0x00FFFFFF; }

        public bool IsIndexed { get => isIndexed; }

        public byte Red { get => (byte)value; }

        public byte Green { get => (byte)(value >> 8); }

        public byte Blue { get => (byte)(value >> 16); }

        public byte Alpha { get => (byte)(value >> 24); }

        public readonly Color ToColor()
        {
            if (!IsIndexed) {
                throw new FormatException("Pixel is indexed");
            }

            return Color.FromArgb(Alpha, Red, Green, Blue);
        }
    }
}
