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
namespace Texim.Colors
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Color binary encoding interface.
    /// </summary>
    public interface IColorEncoding
    {
        /// <summary>
        /// Decode a single color from a stream of data.
        /// </summary>
        /// <param name="stream">The stream to read the binary encoded color.</param>
        /// <returns>The decoded color.</returns>
        Rgb Decode(Stream stream);

        /// <summary>
        /// Decode several colors from a stream of data.
        /// </summary>
        /// <param name="stream">The stream to read the binary encoded colors.</param>
        /// <param name="numColors">The number of colors to decode.</param>
        /// <returns>The sequence of decoded colors.</returns>
        Rgb[] Decode(Stream stream, int numColors);

        /// <summary>
        /// Decode as many colors as possible from the span of data.
        /// </summary>
        /// <param name="data">The sequence of data to read the binary encoded colors.</param>
        /// <returns>The sequence of decoded colors.</returns>
        Rgb[] Decode(Span<byte> data);

        /// <summary>
        /// Encode a single color.
        /// </summary>
        /// <param name="color">The color to binary encode.</param>
        /// <returns>The binary encoded color.</returns>
        byte[] Encode(Rgb color);

        /// <summary>
        /// Encode a sequence of colors.
        /// </summary>
        /// <param name="colors">The sequence of colors to binary encode.</param>
        /// <returns>The binary encoded colors.</returns>
        byte[] Encode(IEnumerable<Rgb> colors);
    }
}
