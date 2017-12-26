//
// ByteArrayExtension.cs
//
// Author:
//       Benito Palacios Sanchez <benito356@gmail.com>
//
// Copyright (c) 2017 Benito Palacios Sanchez
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace Texim
{
    using System;

    static class ByteArrayExtension
    {
        public static uint GetBits(this byte[] data, ref int bitPos, int size)
        {
            if (size < 0 || size > 32)
                throw new ArgumentOutOfRangeException(nameof(size));

            if (bitPos + size > data.Length * 8)
                throw new IndexOutOfRangeException();

            uint value = 0;
            for (int s = 0; s < size; s++, bitPos++) {
                uint bit = data[bitPos / 8];
                bit >>= (bitPos % 8);
                bit &= 1;

                value |= bit << s;
            }

            return value;
        }

        public static void SetBits(this byte[] data, ref int bitPos, int size, uint value)
        {
            if (size < 0 || size > 32)
                throw new ArgumentOutOfRangeException(nameof(size));

            if (bitPos + size > data.Length * 8)
                throw new IndexOutOfRangeException();

            for (int s = 0; s < size; s++, bitPos++) {
                uint bit = (value >> s) & 1;

                uint val = data[bitPos / 8];
                val |= bit << (bitPos % 8);
                data[bitPos / 8] = (byte)val;
            }
        }
    }
}

