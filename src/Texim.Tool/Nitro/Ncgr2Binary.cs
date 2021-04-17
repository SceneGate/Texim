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
    using System.Collections.Generic;
    using Texim.Pixels;
    using Yarhl.IO;

    public class Ncgr2Binary : NitroSerializer<Ncgr>
    {
        public Ncgr2Binary()
        {
            RegisterSection("CHAR", WriteChar);
            RegisterSection("CPOS", WriteCpos);
        }

        protected override string Stamp => "NCGR";

        private void WriteChar(DataWriter writer, Ncgr model)
        {
            writer.Write((ushort)(model.Height / 8));
            writer.Write((ushort)(model.Width / 8));
            writer.Write((uint)model.Format);
            writer.Write((uint)model.TileMapping);

            int flags = model.IsTiled ? 0 : 1;
            flags |= (model.IsVramTransfer ? 1 : 0) << 8;
            writer.Write(flags);

            IEnumerable<IndexedPixel> pixels = model.Pixels;
            if (model.IsTiled) {
                var swizzling = new TileSwizzling<IndexedPixel>(model.Width);
                pixels = swizzling.Swizzle(pixels);
            }

            IIndexedPixelEncoding encoder = (model.Format == NitroTextureFormat.Indexed4Bpp)
                ? Indexed4Bpp.Instance
                : Indexed8Bpp.Instance;
            byte[] data = encoder.Encode(pixels);

            writer.Write(data.Length);
            writer.Write(0x10); // relative position to data
            writer.Write(data);
        }

        private void WriteCpos(DataWriter writer, Ncgr model)
        {
            writer.Write(model.SourceX);
            writer.Write(model.SourceY);
            writer.Write(model.SourceWidth);
            writer.Write(model.SourceHeight);
        }
    }
}
