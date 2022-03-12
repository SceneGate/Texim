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
    using System;
    using Texim.Compressions.Nitro;

    public class Nscr : IScreenMap, INitroFormat
    {
        public Nscr()
        {
            Version = new Version(1, 0);
        }

        public Nscr(Nscr nscr)
        {
            Version = nscr.Version;
            Maps = nscr.Maps;
            Width = nscr.Width;
            Height = nscr.Height;
            PaletteMode = nscr.PaletteMode;
            BackgroundMode = nscr.BackgroundMode;
        }

        public Nscr(IScreenMap screenMap)
            : this()
        {
            Width = screenMap.Width;
            Height = screenMap.Height;
            Maps = screenMap.Maps;
        }

        public Nscr(Nscr nscr, IScreenMap screenMap)
            : this(nscr)
        {
            Maps = screenMap.Maps;
            Width = screenMap.Width;
            Height = screenMap.Height;
        }

        public Nscr(int width, int height)
            : this()
        {
            Width = width;
            Height = height;
            Maps = new MapInfo[width * height];
        }

        public Version Version { get; set; }

        public MapInfo[] Maps { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public NitroPaletteMode PaletteMode { get; set; }

        public NitroBackgroundMode BackgroundMode { get; set; }

        public byte[] UserExtendedInfo { get; set; }
    }
}
