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
using System.CommandLine;
using System.CommandLine.Invocation;
using Yarhl.FileSystem;

namespace Texim.Tool.JumpUltimateStars
{
    public static class CommandLine
    {
        public static Command CreateCommand()
        {
            var export = new Command("export", "Export image") {
                new Option<string>("--package", "the input koma.aar package", ArgumentArity.ExactlyOne),
                new Option<string>("--koma", "the koma.bin file", ArgumentArity.ExactlyOne),
                new Option<string>("--kshape", "the kshape.bin file", ArgumentArity.ExactlyOne),
                new Option<string>("--output", "the output folder", ArgumentArity.ExactlyOne),
            };
            export.Handler = CommandHandler.Create<string, string, string, string>(Export);

            return new Command("jus", "Jump Ultimate Stars game") {
                export,
            };
        }

        private static void Export(string package, string koma, string kshape, string output)
        {
            var komaFormat = NodeFactory.FromFile(koma)
                .TransformWith<Binary2Koma>()
                .GetFormatAs<Koma>();

            var images = NodeFactory.FromFile(package)
                .TransformWith<BinaryAlar2Container>();
        }
    }
}
