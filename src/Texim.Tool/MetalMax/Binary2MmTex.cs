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
namespace Texim.Tool.MetalMax
{
    using System;
    using Texim.Colors;
    using Texim.Pixels;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public class Binary2MmTex :
        IConverter<IBinary, MmTex>,
        IConverter<MmTex, BinaryFormat>
    {
        public MmTex Convert(IBinary source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            source.Stream.Position = 0;
            var reader = new DataReader(source.Stream);

            var texture = new MmTex();
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
                        break;

                    case "TXIM":
                        texture.Pixels = reader.ReadBytes(size).DecodePixelsAs<IndexedA5I3>();
                        break;

                    case "TXPL":
                        texture.Colors.Add(reader.ReadBytes(size).DecodeColorsAs<Bgr555>());
                        break;

                    default:
                        throw new FormatException("Unknown chunk");
                }
            }

            return texture;
        }

        public BinaryFormat Convert(MmTex source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            BinaryFormat binary = new BinaryFormat();
            DataWriter writer = new DataWriter(binary.Stream);

            var colorsData = Bgr555.Instance.Encode(source.Colors);
            var imgData = source.Pixels.EncodePixelsAs<IndexedA5I3>();

            // First texture info
            writer.Write("CHNK", false);
            writer.Write(0x00);
            writer.Write("TXIF", false);
            writer.Write(0x18);

            writer.Write(source.UnknownSize);
            writer.Write(imgData.Length);
            writer.Write(source.Unknown);
            writer.Write(colorsData.Length);
            writer.Write(source.Width);
            writer.Write(source.Height);
            writer.Write(source.NumImages);

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
