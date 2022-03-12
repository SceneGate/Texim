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
namespace Texim.Games.DevilSurvivor
{
    using System;
    using Texim.Colors;
    using Texim.Palettes;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public class DsTex2Palette : IConverter<IBinary, Palette>
    {
        private const int NumColors = 256;

        public Palette Convert(IBinary source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            source.Stream.Position = 0;
            DataReader reader = new DataReader(source.Stream);

            var colors = reader.ReadColors<Bgr555>(NumColors);
            return new Palette(colors);
        }
    }
}
