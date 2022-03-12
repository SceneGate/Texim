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
    using System.Collections.Generic;
    using System.Linq;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public abstract class NitroDeserializer<T> : IConverter<IBinary, T>
        where T : INitroFormat, new()
    {
        protected abstract string Stamp { get; }

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

        protected IEnumerable<string> ReadLabelSection(DataReader reader)
        {
            // Reading this section is very tricky as there isn't a number of elements
            // or a way to map each name to an entry of the format.
            // There is an offset table and then a list of null-terminated strings.
            // Each offset is relative to the start of list of strings, so there isn't
            // a way to determine how many offsets or where is the last one.
            // We keep reading, assuming each offset is incremental and not larger than
            // 128 bytes (in that case we may have read 4 bytes of the string).
            var offsets = new List<int>();
            int lastOffset = 0;
            while (!reader.Stream.EndOfStream) {
                int offset = reader.ReadInt32();
                if (offset < lastOffset || offset > lastOffset + 128) {
                    reader.Stream.Position -= 4;
                    break;
                }

                offsets.Add(offset);
                lastOffset = offset;
            }

            long dataPosition = reader.Stream.Position;
            foreach (int offset in offsets) {
                reader.Stream.Position = dataPosition + offset;
                yield return reader.ReadString();
            }
        }

        private void ReadHeader(DataReader reader, T model, out int numSections)
        {
            string stamp = new string(reader.ReadChars(4).Reverse().ToArray());
            if (stamp != Stamp) {
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

                if (id == "UEXT") {
                    ReadUserExtendedInfo(reader, model, size);
                } else {
                    ReadSection(reader, model, id, size);
                }

                reader.Stream.Position = sectionPosition + size;
            }
        }

        private void ReadUserExtendedInfo(DataReader reader, T model, int size)
        {
            int dataSize = size - 8;
            model.UserExtendedInfo = reader.ReadBytes(dataSize);
        }
    }
}
