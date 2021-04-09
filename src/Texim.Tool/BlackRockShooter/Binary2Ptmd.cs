//
// Binary2Ptmd.cs
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
namespace Texim.BlackRockShooter
{
    using System;
    using System.Drawing;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public class Binary2Ptmd : IConverter<BinaryFormat, Ptmd>
    {
        public Ptmd Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            DataReader reader = new DataReader(source.Stream);
            if (reader.ReadString(4) != "PTMD")
                throw new FormatException("Invalid magic header");

            Ptmd ptp = new Ptmd();
            reader.ReadUInt32(); // unknown
            reader.ReadUInt32(); // unknown
            reader.ReadUInt32(); // unknown

            ptp.Pixels = new PixelArray {
                Width = 256,
                Height = 128,
            };
            ptp.Pixels.SetData(
                reader.ReadBytes(0x2000),
                PixelEncoding.HorizontalTiles,
                ColorFormat.Indexed_4bpp,
                new Size(32, 8));

            // It isn't the palette but it could work for now
            reader.Stream.Position = 0x2010;
            ptp.Palette = new Palette(reader.ReadBytes(0x20).ToBgr555Colors());

            // Maybe they are coords? Let's try again with big endian shorts
            Console.WriteLine("Coordinates1");
            reader.Endianness = EndiannessMode.BigEndian;
            reader.Stream.Position = 0x2010;
            for (int i = 0; i < 16; i++)
                Console.WriteLine(reader.ReadInt16());

            // Or maybe coordinates with signed bytes?
            Console.WriteLine("Coordinates2:");
            reader.Stream.Position = 0x2010;
            for (int i = 0; i < 32; i++)
                Console.WriteLine(reader.ReadSByte());

            return ptp;
        }
    }
}
