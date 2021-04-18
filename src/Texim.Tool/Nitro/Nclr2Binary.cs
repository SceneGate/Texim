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
namespace Texim.Tool.Nitro
{
    using System;
    using System.Linq;
    using Texim.Colors;
    using Yarhl.IO;

    public class Nclr2Binary : NitroSerializer<Nclr>
    {
        public Nclr2Binary()
        {
            RegisterSection("PLTT", WritePltt);
            RegisterSection("PCMP", WritePcmp);
        }

        protected override string Stamp => "NCLR";

        private void WritePltt(DataWriter writer, Nclr model)
        {
            writer.Write((uint)model.TextureFormat);
            writer.Write(model.IsExtendedPalette ? 1 : 0);
            writer.Write(model.Palettes.Sum(p => p.Colors.Count) * 2);
            writer.Write(0x0C); // relative offset to data

            var colors = model.Palettes.SelectMany(p => p.Colors);
            writer.Write<Bgr555>(colors);
        }

        private void WritePcmp(DataWriter writer, Nclr model)
        {
            // TODO: How to compress??
            writer.Write((ushort)model.Palettes.Count);
            writer.Write((ushort)0xBEEF); // padding
            writer.Write(0x08); // relative offset

            for (int i = 0; i < model.Palettes.Count; i++) {
                writer.Write((ushort)i);
            }
        }
    }
}
