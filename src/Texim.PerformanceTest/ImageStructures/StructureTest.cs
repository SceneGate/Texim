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
namespace Texim.PerformanceTest.ImageStructures
{
    using BenchmarkDotNet.Attributes;

    [MemoryDiagnoser]
    public class StructureTest
    {
        ushort[] colorBgr555;
        byte[] index;

        [Params(192 * 256, 4096 * 2160)]
        public int Size { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            colorBgr555 = new ushort[] { 0xCAFE };
            index = new byte[] { 0xCA, 0xFE };
        }

        [Benchmark]
        public int SetPrimitiveIndexed()
        {
            var image = new uint[Size];
            for (int i = 0; i < Size; i++) {
                uint color = (uint)(index[0] | (index[1] << 24));
                image[i] = color;
            }

            return -1;
        }

        [Benchmark]
        public int SetPixelIndexed()
        {
            var image = new Pixel[Size];
            for (int i = 0; i < Size; i++) {
                var color = new Pixel(index[0], index[1]);
                image[i] = color;
            }

            return -1;
        }

        [Benchmark]
        public int SetPixelRgbIndexed()
        {
            var image = new PixelRgb[Size];
            for (int i = 0; i < Size; i++) {
                var color = new PixelRgb(index[0], index[1]);
                image[i] = color;
            }

            return -1;
        }

        [Benchmark]
        public int SetPrimitiveColor()
        {
            var image = new uint[Size];
            for (int i = 0; i < Size; i++) {
                uint color = (uint)(
                    ((colorBgr555[0] & 0xFF) << 3) |
                    ((((colorBgr555[0] >> 5) & 0xFF) << 3) << 8) |
                    ((((colorBgr555[0] >> 10) & 0xFF) << 3) << 16) |
                    (((colorBgr555[0] >> 15) * 255) << 24));
                image[i] = color;
            }

            return -1;
        }

        [Benchmark]
        public int SetPixelColor()
        {
            var image = new Pixel[Size];
            for (int i = 0; i < Size; i++) {
                var color = new Pixel(
                    (byte)((colorBgr555[0] & 0xFF) << 3),
                    (byte)(((colorBgr555[0] >> 5) & 0xFF) << 3),
                    (byte)(((colorBgr555[0] >> 10) & 0xFF) << 3),
                    (byte)((colorBgr555[0] >> 15) * 255));
                image[i] = color;
            }

            return -1;
        }

        [Benchmark]
        public int SetPixelRgbColor()
        {
            var image = new PixelRgb[Size];
            for (int i = 0; i < Size; i++) {
                var color = new PixelRgb(
                    (byte)((colorBgr555[0] & 0xFF) << 3),
                    (byte)(((colorBgr555[0] >> 5) & 0xFF) << 3),
                    (byte)(((colorBgr555[0] >> 10) & 0xFF) << 3),
                    (byte)((colorBgr555[0] >> 15) * 255));
                image[i] = color;
            }

            return -1;
        }

        [Benchmark]
        public int GetPrimitiveIndex()
        {
            int sum = 0;
            var image = new uint[Size];
            for (int i = 0; i < Size; i++) {
                var pixel = image[i];
                ushort color = (ushort)pixel;
                sum += color;
            }

            return sum;
        }

        [Benchmark]
        public int GetPixelIndex()
        {
            int sum = 0;
            var image = new Pixel[Size];
            for (int i = 0; i < Size; i++) {
                var pixel = image[i];
                ushort color = (ushort)pixel.Index;
                sum += color;
            }

            return sum;
        }

        [Benchmark]
        public int GetPixelRgbIndex()
        {
            int sum = 0;
            var image = new PixelRgb[Size];
            for (int i = 0; i < Size; i++) {
                var pixel = image[i];
                ushort color = pixel.Index;
                sum += color;
            }

            return sum;
        }

        [Benchmark]
        public int GetPrimitiveColor()
        {
            int sum = 0;
            var image = new uint[Size];
            for (int i = 0; i < Size; i++) {
                var pixel = image[i];
                ushort color = (ushort)(((pixel & 0xFF) >> 3)
                    | ((((pixel >> 8) & 0xFF) >> 3) << 5)
                    | ((((pixel >> 16) & 0xFF) >> 3) << 10)
                    | (((pixel >> 24) > 0 ? 1u : 0u) << 15));
                sum += color;
            }

            return sum;
        }

        [Benchmark]
        public int GetPixelColor()
        {
            int sum = 0;
            var image = new Pixel[Size];
            for (int i = 0; i < Size; i++) {
                var pixel = image[i];
                ushort color = (ushort)((pixel.Red >> 3)
                    | ((pixel.Green >> 3) << 5)
                    | ((pixel.Blue >> 3) << 10)
                    | ((pixel.Alpha > 0 ? 1 : 0) << 15));
                sum += color;
            }

            return sum;
        }

        [Benchmark]
        public int GetPixelRgbColor()
        {
            int sum = 0;
            var image = new PixelRgb[Size];
            for (int i = 0; i < Size; i++) {
                var pixel = image[i];
                ushort color = (ushort)((pixel.Red >> 3)
                    | ((pixel.Green >> 3) << 5)
                    | ((pixel.Blue >> 3) << 10)
                    | ((pixel.Alpha > 0 ? 1 : 0) << 15));
                sum += color;
            }

            return sum;
        }
    }
}
