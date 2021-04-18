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
    using Texim.Pixels;
    using Yarhl.IO;

    public class Binary2Ncgr : NitroDeserializer<Ncgr>
    {
        protected override string Stamp => "NCGR";

        protected override void ReadSection(DataReader reader, Ncgr model, string id, int size)
        {
            switch (id) {
                case "CHAR":
                    ReadChar(reader, model);
                    break;

                case "CPOS":
                    ReadCpos(reader, model);
                    break;

                default:
                    throw new FormatException($"Unknown section: {id}");
            }
        }

        private void ReadChar(DataReader reader, Ncgr model)
        {
            long sectionPos = reader.Stream.Position;

            model.Height = reader.ReadUInt16();
            model.Width = reader.ReadUInt16();

            model.Format = (NitroTextureFormat)reader.ReadUInt32();
            model.TileMapping = (TileMappingKind)reader.ReadInt32();

            uint flags = reader.ReadUInt32();
            model.IsTiled = (flags & 0xFF) == 0;
            model.IsVramTransfer = (flags >> 8) == 1;

            int pixelSize = reader.ReadInt32();
            uint pixelOffset = reader.ReadUInt32();

            reader.Stream.Position = sectionPos + pixelOffset;
            byte[] pixelData = reader.ReadBytes(pixelSize);

            // This format only supports 4bpp and 8bpp.
            IIndexedPixelEncoding decoder = (model.Format == NitroTextureFormat.Indexed4Bpp)
                ? Indexed4Bpp.Instance
                : Indexed8Bpp.Instance;
            IndexedPixel[] pixels = decoder.Decode(pixelData);

            // Size is in tile units but it can be invalid if there is a third
            // file NCER or NSCR. In that case we assume the width is just one
            // tile, so 8.
            if (model.Width == 0xFFFF || model.Height == 0xFFFF) {
                model.Width = 8; // tile width
                model.Height = pixels.Length / model.Width;
            } else {
                model.Width *= 8;
                model.Height *= 8;
            }

            if (model.IsTiled) {
                var swizzling = new TileSwizzling<IndexedPixel>(model.Width);
                pixels = swizzling.Unswizzle(pixels);
            }

            model.Pixels = pixels;
        }

        private void ReadCpos(DataReader reader, Ncgr model)
        {
            model.SourceX = reader.ReadUInt16();
            model.SourceY = reader.ReadUInt16();
            model.SourceWidth = reader.ReadUInt16();
            model.SourceHeight = reader.ReadUInt16();
        }
    }
}
