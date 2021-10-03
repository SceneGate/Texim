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
using System.Collections.ObjectModel;
using Yarhl.FileFormat;

namespace Texim.Tool.JumpUltimateStars
{
    public class Koma : Collection<KomaElement>, IFormat
    {
        public static string[] NameTable { get; } = new[] {
            null, "es", "jj", "op", "rb", "rk", "bl", "yo", "mr", "mo",
            "nn", "bb", "hk", "hs", "yh", "bc", "bu", "pj", "hh", "nk",
            "na", "db", "tl", "ds", "dn", "dg", "to", "tz", "ss", "sd",
            "dt", "tc", "sk", "nb", "oj", "cb", "kk", "kn", "gt", "ct",
            "tr", "ig", "is",
        };
    }
}
