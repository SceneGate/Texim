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
    using System.Threading.Tasks;

    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            var root = new RootCommand("Proof-of-concept library and tool for image formats");
            root.Add(Nitro.CommandLine.CreateCommand());
            root.Add(BlackRockShooter.CommandLine.CreateCommand());
            root.Add(DevilSurvivor.CommandLine.CreateCommand());
            root.Add(Disgaea.CommandLine.CreateCommand());
            root.Add(MetalMax.CommandLine.CreateCommand());
            root.Add(LondonLife.CommandLine.CreateCommand());
            root.Add(Raw.CommandLine.CreateCommand());

            return root.InvokeAsync(args);
        }
    }
}
