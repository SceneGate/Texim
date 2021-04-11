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
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public abstract class NitroDeserializer<T> : IConverter<IBinary, T>
        where T : INitroFormat, new()
    {
        public T Convert(IBinary source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            source.Stream.Position = 0;
            var reader = new DataReader(source.Stream);

            T model = new T();
            ReadHeader(reader, model, out int numSections);
            ReadSections(reader, model, numSections);
            PostProcessing(model);

            return model;
        }

        protected abstract void ReadSection(DataReader reader, T model, string id, int size);

        protected virtual void PostProcessing(T model)
        {
        }

        private void ReadHeader(DataReader reader, T model, out int numSections)
        {
            string stamp = new string(reader.ReadChars(4).Reverse().ToArray());
            if (stamp != model.Stamp) {
                throw new FormatException($"Invalid stamp: {stamp}");
            }

            ushort endianness = reader.ReadUInt16();
            if (endianness == 0xFFFE) {
                reader.Endianness = EndiannessMode.BigEndian;
            } else if (endianness != 0xFEFF) {
                throw new FormatException($"Invalid endianness: {endianness:X4}");
            }

            ushort version = reader.ReadUInt16();
            model.Version = new Version(version >> 8, version & 0xFF);

            uint totalSize = reader.ReadUInt32();
            if (reader.Stream.Length < totalSize) {
                throw new FormatException("Missing data in the file");
            }

            ushort headerSize = reader.ReadUInt16();
            numSections = reader.ReadUInt16();

            reader.Stream.Position = headerSize;
        }

        private void ReadSections(DataReader reader, T model, int numSections)
        {
            for (int i = 0; i < numSections; i++) {
                long sectionPosition = reader.Stream.Position;

                string id = new string(reader.ReadChars(4).Reverse().ToArray());
                int size = reader.ReadInt32();

                ReadSection(reader, model, id, size);

                reader.Stream.Position = sectionPosition + size;
            }
        }
    }
}
