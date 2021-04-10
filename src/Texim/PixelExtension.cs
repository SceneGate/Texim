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
namespace Texim
{
    using System.Drawing;

    public static class PixelExtension
    {
        public static void FlipX(this Pixel[] tile, Size tileSize)
        {
            for (int y = 0; y < tileSize.Height; y++) {
                for (int x = 0; x < tileSize.Width / 2; x++) {
                    int t1 = y * tileSize.Width + x;
                    int t2 = y * tileSize.Width + (tileSize.Width - 1 - x);

                    Pixel swap = tile[t1];
                    tile[t1] = tile[t2];
                    tile[t2] = swap;
                }
            }
        }

        public static void FlipY(this Pixel[] tile, Size tileSize)
        {
            for (int x = 0; x < tileSize.Width; x++) {
                for (int y = 0; y < tileSize.Height / 2; y++) {
                    int t1 = x + tileSize.Width * y;
                    int t2 = x + tileSize.Width * (tileSize.Height - 1 - y);

                    Pixel swap = tile[t1];
                    tile[t1] = tile[t2];
                    tile[t2] = swap;
                }
            }
        }
    }
}
