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
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using Texim.Games.Raw;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public static class RawCommandLine
    {
        public static Command CreateCommand()
        {
            var export = new Command("export", "Export raw images") {
                new Option<string>("--config", "path to the YAML configuration file", ArgumentArity.ExactlyOne),
                new Option<string>("--input-path", "base path for input files", ArgumentArity.ZeroOrOne),
                new Option<string>("--output-path", "base path for output files", ArgumentArity.ZeroOrOne),
            };
            export.Handler = CommandHandler.Create<string, string, string>(Export);

            return new Command("raw", "Raw formats") {
                export,
            };
        }

        private static void Export(string config, string inputPath, string outputPath)
        {
            using var configReader = new StreamReader(config);
            var configurations = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build()
                .Deserialize<IEnumerable<RawConfiguration>>(configReader);

            foreach (var configObj in configurations) {
                Exporter.Export(configObj, inputPath, outputPath);
            }
        }
    }
}
