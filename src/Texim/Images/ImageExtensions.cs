// Copyright (c) 2023 SceneGate

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
namespace Texim.Images;

public static class ImageExtensions
{
    public static FullImage SubImage(this IFullImage image, int startX, int startY, int width, int height)
    {
        var subImage = new FullImage(width, height);
        CopySubImage(image.Pixels, subImage.Pixels, image.Width, startX, startY, width, height);
        return subImage;
    }

    public static IndexedImage SubImage(this IIndexedImage image, int startX, int startY, int width, int height)
    {
        var subImage = new IndexedImage(width, height);
        CopySubImage(image.Pixels, subImage.Pixels, image.Width, startX, startY, width, height);
        return subImage;
    }

    private static void CopySubImage<T>(T[] source, T[] destination, int sourceWidth, int startX, int startY, int width, int height)
    {
        int idx = 0;
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                int fullIndex = ((startY + y) * sourceWidth) + startX + x;
                destination[idx++] = source[fullIndex];
            }
        }
    }
}
