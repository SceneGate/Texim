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
namespace Texim.Tool
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using Texim.Formats;
    using Texim.Games.Disgaea;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    public static class DisgaeaCommandLine
    {
        public static Command CreateCommand()
        {
            var export = new Command("export", "Export image") {
                new Option<string>("--input", "the input file", ArgumentArity.ExactlyOne),
                new Option<string>("--output", "the output file", ArgumentArity.ExactlyOne),
            };
            export.Handler = CommandHandler.Create<string, string>(Export);

            return new Command("disgaea", "Disgaea game") {
                export,
            };
        }

        private static void Export(string input, string output)
        {
            NodeFactory.FromFile(input, FileOpenMode.Read)
                .TransformWith<Ykcmp2Image>()
                .TransformWith<FullImage2Bitmap>()
                .Stream.WriteTo(output);
        }
    }
}
