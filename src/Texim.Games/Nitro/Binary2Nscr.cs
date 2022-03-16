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

    public class Binary2Nscr : NitroDeserializer<Nscr>
    {
        protected override string Stamp => "NSCR";

        protected override void ReadSection(DataReader reader, Nscr model, string id, int size)
        {
            if (id == "SCRN") {
                ReadScrn(reader, model);
            }
        }

        private void ReadScrn(DataReader reader, Nscr model)
        {
            model.Width = reader.ReadUInt16();
            model.Height = reader.ReadUInt16();
            model.PaletteMode = (NitroPaletteMode)reader.ReadUInt16();
            model.BackgroundMode = (NitroBackgroundMode)reader.ReadUInt16();

            int length = reader.ReadInt32();
            if (model.BackgroundMode == NitroBackgroundMode.Affine) {
                model.Maps = reader.ReadBytes(length)
                    .Select(b => new MapInfo(b))
                    .ToArray();
            } else {
                model.Maps = reader.ReadMapInfos(length / 2);
            }
        }
    }
}
