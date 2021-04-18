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
    using Texim.Images;
    using Texim.Pixels;

    public class Ncgr : IIndexedImage, INitroFormat
    {
        public Ncgr()
        {
            Version = new Version(1, 0);
        }

        public Ncgr(Ncgr ncgr)
        {
            Version = ncgr.Version;
            Height = ncgr.Height;
            Width = ncgr.Width;
            Pixels = ncgr.Pixels.ToArray();
            Format = ncgr.Format;
            TileMapping = ncgr.TileMapping;
            IsTiled = ncgr.IsTiled;
            IsVramTransfer = ncgr.IsVramTransfer;
            SourceWidth = ncgr.SourceWidth;
            SourceHeight = ncgr.SourceHeight;
            SourceX = ncgr.SourceX;
            SourceY = ncgr.SourceY;
        }

        public Ncgr(IIndexedImage image)
            : this()
        {
            Height = image.Height;
            Width = image.Width;
            Pixels = image.Pixels.ToArray();
        }

        public Ncgr(Ncgr ncgr, IIndexedImage image)
            : this(ncgr)
        {
            Height = image.Height;
            Width = image.Width;
            Pixels = image.Pixels.ToArray();
        }

        public Ncgr(int width, int height)
            : this()
        {
            Width = width;
            Height = height;
            Pixels = new IndexedPixel[width * height];
        }

        public Version Version { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public IndexedPixel[] Pixels { get; set; }

        public NitroTextureFormat Format { get; set; }

        public TileMappingKind TileMapping { get; set; }

        public bool IsTiled { get; set; }

        public bool IsVramTransfer { get; set; }

        public int SourceWidth { get; set; }

        public int SourceHeight { get; set; }

        public int SourceX { get; set; }

        public int SourceY { get; set; }
    }
}
