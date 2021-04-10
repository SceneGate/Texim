//
// MmTex2Image.cs
//
// Author:
//       Benito Palacios Sanchez <benito356@gmail.com>
//
// Copyright (c) 2017 Benito Palacios Sanchez
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace Texim.MetalMax
{
    using System;
    using Texim.Colors;
    using Texim.Palettes;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public class Binary2MmTex :
        IConverter<BinaryFormat, MmTex>,
        IConverter<MmTex, BinaryFormat>
    {
        public MmTex Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            DataReader reader = new DataReader(source.Stream);

            MmTex texture = new MmTex();
            while (!source.Stream.EndOfStream) {
                reader.ReadString(4); // CHNK
                reader.ReadUInt32(); // unknown
                string header = reader.ReadString(4);
                int size = reader.ReadInt32();

                switch (header) {
                    case "TXIF":
                        texture.UnknownSize = reader.ReadUInt32();
                        reader.ReadUInt32(); // num pixels
                        texture.Unknown = reader.ReadUInt32();
                        reader.ReadUInt32(); // palette size
                        texture.Width = reader.ReadUInt16();
                        texture.Height = reader.ReadUInt16();
                        texture.NumImages = reader.ReadInt32();
                        texture.Pixels = new PixelArray {
                            Width = texture.Width,
                            Height = texture.NumImages * texture.Height
                        };
                        break;

                    case "TXIM":
                        texture.Pixels.SetData(
                            reader.ReadBytes(size),
                            PixelEncoding.Lineal,
                            ColorFormat.Indexed_A5I3);
                        break;

                    case "TXPL":
                        byte[] paletteData = reader.ReadBytes(size);
                        texture.Palette = new Palette(Bgr555.Instance.Decode(paletteData));
                        break;

                    default:
                        throw new FormatException("Unknown chunk");
                }
            }

            return texture;
        }

        public BinaryFormat Convert(MmTex texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            BinaryFormat binary = new BinaryFormat();
            DataWriter writer = new DataWriter(binary.Stream);

            var colorsData = Bgr555.Instance.Encode(texture.Palette.Colors);
            var imgData = texture.Pixels.GetData();

            // First texture info
            writer.Write("CHNK", false);
            writer.Write(0x00);
            writer.Write("TXIF", false);
            writer.Write(0x18);

            writer.Write(texture.UnknownSize);
            writer.Write(imgData.Length);
            writer.Write(texture.Unknown);
            writer.Write(colorsData.Length);
            writer.Write(texture.Width);
            writer.Write(texture.Height);
            writer.Write(texture.NumImages);

            // Image data
            writer.Write("CHNK", false);
            writer.Write(0x00);
            writer.Write("TXIM", false);
            writer.Write(imgData.Length);
            writer.Write(imgData);

            // Palette data
            writer.Write("CHNK", false);
            writer.Write(0x00);
            writer.Write("TXPL", false);
            writer.Write(colorsData.Length);
            writer.Write(colorsData);

            return binary;
        }
    }
}
