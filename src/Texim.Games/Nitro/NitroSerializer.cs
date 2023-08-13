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
    using System.IO;
    using System.Linq;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public abstract class NitroSerializer<T> : IConverter<T, BinaryFormat>
        where T : INitroFormat
    {
        private readonly List<(string Id, Action<DataWriter, T> Writer)> registeredSections = new();

        protected abstract string Stamp { get; }

        public BinaryFormat Convert(T source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var binary = new BinaryFormat();
            var writer = new DataWriter(binary.Stream);

            var sections = registeredSections.ToList();
            if (source.UserExtendedInfo is { Length: > 0 }) {
                sections.Add(("UEXT", WriteUserExtendedInfo));
            }

            WriteHeader(writer, source, sections);
            WriteSections(writer, source, sections);
            UpdateFileSize(writer);

            return binary;
        }

        protected void RegisterSection(string id, Action<DataWriter, T> writer)
        {
            registeredSections.Add((id, writer));
        }

        protected void WriteLabelSection(DataWriter writer, IEnumerable<string> labels)
        {
            string[] labelList = labels.ToArray();
            long sectionPos = writer.Stream.Position;

            // pre-fill offset table
            writer.WriteTimes(0x00, labelList.Length * sizeof(uint));

            // write each table with the table
            long dataPos = writer.Stream.Position;
            for (int i = 0; i < labelList.Length; i++) {
                uint labelOffset = (uint)(writer.Stream.Position - dataPos);

                writer.Stream.PushToPosition(sectionPos + (i * sizeof(uint)));
                writer.Write(labelOffset);
                writer.Stream.PopPosition();

                writer.Write(labelList[i]);
            }
        }

        private void WriteHeader(DataWriter writer, T model, List<(string Id, Action<DataWriter, T> Writer)> sections)
        {
            writer.Write(Stamp.ToCharArray().Reverse().ToArray());
            writer.Write((ushort)0xFEFF); // endianness
            writer.Write((byte)model.Version.Minor);
            writer.Write((byte)model.Version.Major);
            writer.Write(0x00); // placeholder for file size
            writer.Write((ushort)0x10); // header size
            writer.Write((ushort)sections.Count);
        }

        private void UpdateFileSize(DataWriter writer)
        {
            writer.Stream.Position = 0x08;
            writer.Write((uint)writer.Stream.Length);
        }

        private void WriteSections(DataWriter writer, T model, List<(string Id, Action<DataWriter, T> Writer)> sections)
        {
            foreach (var section in sections) {
                writer.Write(section.Id.ToCharArray().Reverse().ToArray());
                writer.Write(0x00);

                long dataPosition = writer.Stream.Position;
                section.Writer.Invoke(writer, model);

                long dataLength = writer.Stream.Length - dataPosition + 8;
                writer.Stream.Position = dataPosition - 4;
                writer.Write((uint)dataLength);

                _ = writer.Stream.Seek(0, SeekOrigin.End);
            }
        }

        private void WriteUserExtendedInfo(DataWriter writer, T model) =>
            writer.Write(model.UserExtendedInfo);
    }
}
