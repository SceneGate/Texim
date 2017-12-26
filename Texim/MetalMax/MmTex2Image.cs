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
    using Yarhl.FileFormat;
    using Yarhl.IO;
    using Media.Image;

    public class MmTex2Image :
        IConverter<BinaryFormat, ValueTuple<Palette, PixelArray>>,
        IConverter<ValueTuple<Palette, PixelArray>, BinaryFormat>
    {
        public (Palette, PixelArray) Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            DataReader reader = new DataReader(source.Stream);

            Palette palette = new Palette();
            PixelArray pixels = new PixelArray();

            while (!source.Stream.EndOfStream) {
                reader.ReadString(4); // CHNK
                reader.ReadUInt32(); // unknown
                string header = reader.ReadString(4);
                int size = reader.ReadInt32();

                switch (header) {
                    case "TXIF":
                        reader.ReadUInt32(); // unknown, some sort of size
                        reader.ReadUInt32(); // num pixels
                        reader.ReadUInt32(); // unknown / reserved
                        reader.ReadUInt32(); // palette size
                        ushort width = reader.ReadUInt16();
                        ushort height = reader.ReadUInt16();
                        int numImgs = reader.ReadInt32();
                        pixels.Width = width;
                        pixels.Height = numImgs * height;
                        break;

                    case "TXIM":
                        pixels.SetData(
                            reader.ReadBytes(size),
                            PixelEncoding.Lineal,
                            ColorFormat.Indexed_A5I3);
                        break;

                    case "TXPL":
                        palette.SetPalette(reader.ReadBytes(size).ToBgr555Colors());
                        break;
                }
            }

            return (palette, pixels);
        }

        public BinaryFormat Convert(ValueTuple<Palette, PixelArray> source)
        {
            BinaryFormat binary = new BinaryFormat();
            DataWriter writer = new DataWriter(binary.Stream);

            var colorsData = source.Item1.GetPalette(0).ToBgr555();
            var imgData = source.Item2.GetData();

            // First texture info
            writer.Write("CHNK", false);
            writer.Write(0x00);
            writer.Write("TXIF", false);
            writer.Write(0x18);

            writer.Write(0xD16);
            writer.Write(imgData.Length);
            writer.Write(0x00);
            writer.Write(colorsData.Length);
            writer.Write((ushort)0x20); // TODO: Make configurable
            writer.Write((ushort)0x0E); // TODO: Make configurable
            writer.Write(0x05); // TODO: Make configurable

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
