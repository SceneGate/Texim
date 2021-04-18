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
    using System.Collections.ObjectModel;
    using System.Linq;
    using Texim.Palettes;

    public class Nclr : IPaletteCollection, INitroFormat
    {
        public Nclr()
        {
            Version = new Version(1, 0);
        }

        public Nclr(Nclr nclr)
        {
            Version = nclr.Version;
            IsExtendedPalette = nclr.IsExtendedPalette;
            TextureFormat = nclr.TextureFormat;

            foreach (var palette in nclr.Palettes) {
                Palettes.Add(new Palette(palette.Colors.ToArray()));
            }
        }

        public Nclr(IPaletteCollection palettes)
            : this()
        {
            if (palettes.Palettes.Count > 1) {
                if (palettes.Palettes.All(p => p.Colors.Count == 256)) {
                    IsExtendedPalette = true;
                    TextureFormat = NitroTextureFormat.Indexed8Bpp;
                } else if (palettes.Palettes.Any(p => p.Colors.Count != 16)) {
                    throw new ArgumentOutOfRangeException("Multi-palettes must have 16 or 256 colors");
                } else {
                    IsExtendedPalette = false;
                    TextureFormat = NitroTextureFormat.Indexed4Bpp;
                }
            } else if (palettes.Palettes.Count == 1) {
                IsExtendedPalette = false;
                TextureFormat = (palettes.Palettes[0].Colors.Count > 16)
                    ? NitroTextureFormat.Indexed8Bpp
                    : NitroTextureFormat.Indexed4Bpp;
            }

            foreach (var palette in palettes.Palettes) {
                Palettes.Add(new Palette(palette.Colors.ToArray()));
            }
        }

        public Nclr(IPalette palette)
            : this()
        {
            IsExtendedPalette = false;
            TextureFormat = (palette.Colors.Count > 16)
                ? NitroTextureFormat.Indexed8Bpp
                : NitroTextureFormat.Indexed4Bpp;
            Palettes.Add(new Palette(palette.Colors.ToArray()));
        }

        public Version Version { get; set; }

        public Collection<IPalette> Palettes { get; } = new Collection<IPalette>();

        public bool IsExtendedPalette { get; set; }

        public NitroTextureFormat TextureFormat { get; set; }
    }
}
