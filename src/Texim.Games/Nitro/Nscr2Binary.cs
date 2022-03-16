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
namespace Texim.Games.Nitro
{
    using System.Linq;
    using Texim.Compressions.Nitro;
    using Yarhl.IO;

    public class Nscr2Binary : NitroSerializer<Nscr>
    {
        public Nscr2Binary()
        {
            RegisterSection("SCRN", WriteScrn);
        }

        protected override string Stamp => "NSCR";

        private void WriteScrn(DataWriter writer, Nscr model)
        {
            writer.Write((ushort)model.Width);
            writer.Write((ushort)model.Height);
            writer.Write((ushort)model.PaletteMode);
            writer.Write((ushort)model.BackgroundMode);

            if (model.BackgroundMode == NitroBackgroundMode.Affine) {
                writer.Write(model.Maps.Length);
                writer.Write(model.Maps.Select(m => (byte)m.TileIndex).ToArray());
            } else {
                writer.Write(model.Maps.Length * 2);
                writer.Write(model.Maps);
            }
        }
    }
}
