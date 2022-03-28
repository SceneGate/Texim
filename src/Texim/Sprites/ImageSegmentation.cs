// Copyright (c) 2022 SceneGate

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
namespace Texim.Sprites;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Colors;
using Texim.Images;
using Texim.Pixels;

public class ImageSegmentation : IImageSegmentation
{
    // First value is the limit and the second is the side.
    // From limit to side there must be non-transparent pixels to set it.
    private static readonly int[][,] Modes = {
        new int[,] { { 32, 64 }, { 16, 32 }, { 8, 16 }, { 0, 8 } }, // 50%
        new int[,] { { 48, 64 }, { 24, 32 }, { 8, 16 }, { 0, 8 } }, // 75%
    };

    private readonly int[,] splitMode = Modes[1];

    public (Sprite, FullImage) Segment(FullImage frame)
    {
        (int startX, int startY, FullImage trimmed) = TrimImage(frame);

        var segments = CreateObjects(trimmed, startX, startY, 0, 0, trimmed.Height);

        // Return new frame
        var sprite = new Sprite {
            Segments = new Collection<IImageSegment>(segments),
            Width = trimmed.Width,
            Height = trimmed.Height,
        };
        return (sprite, trimmed);
    }

    private List<IImageSegment> CreateObjects(FullImage frame, int startX, int startY, int x, int y, int maxHeight)
    {
        var segments = new List<IImageSegment>();

        // Go to first non-transparent pixel
        int newX = SearchNoTransparentPoint(frame, 1, x, y, yEnd: y + maxHeight);
        int newY = SearchNoTransparentPoint(frame, 0, x, y, yEnd: y + maxHeight);

        if (newY == -1 || newX == -1) {
            return segments;
        }

        int diffX = newX - x;
        diffX -= diffX % 8;
        x = diffX + x;

        int diffY = newY - y;
        diffY -= diffY % 8;
        y = diffY + y;

        GetObjectSize(frame, x, y, frame.Width, maxHeight, out int width, out int height);

        if (width != 0 && height != 0) {
            var segment = new ImageSegment {
                CoordinateX = startX + x - 256,
                CoordinateY = startY + y - 128,
                Width = width,
                Height = height,
                Layer = 0,
            };
            segments.Add(segment);
        } else {
            // If everything is transparent
            width = splitMode[0, 1];  // Max width
            height = splitMode[0, 1]; // Max height
        }

        // Go to right
        if (frame.Width - (x + width) > 0) {
            segments.AddRange(CreateObjects(frame, startX, startY, x + width, y, height));
        }

        // Go to down
        int newMaxHeight = maxHeight - (height + diffY);
        if (newMaxHeight > 0) {
            segments.AddRange(CreateObjects(frame, startX, startY, x, y + height, newMaxHeight));
        }

        return segments;
    }

    private void GetObjectSize(
        FullImage frame,
        int x,
        int y,
        int maxWidth,
        int maxHeight,
        out int width,
        out int height)
    {
        int minWidthConstraint = 0;
        width = 0;
        height = 0;

        // Try to get a valid object size
        // The problem is the width can get fixed to 64 and in that case the height can not be 8 or 16.
        while (height == 0 && minWidthConstraint < this.splitMode.GetLength(0)) {
            // Get object width
            width = 0;
            for (int i = minWidthConstraint; i < this.splitMode.GetLength(0) && width == 0; i++) {
                if (this.splitMode[i, 1] > maxWidth - x)
                    continue;

                int xRange = this.splitMode[i, 1] - this.splitMode[i, 0];
                if (!IsTransparent(frame, x + this.splitMode[i, 0], xRange, y, maxHeight)) {
                    width = this.splitMode[i, 1];
                }
            }

            // Everything is transparent, skip
            if (width == 0) {
                return;
            }

            // Get object height
            height = 0;
            for (int i = 0; i < this.splitMode.GetLength(0) && height == 0; i++) {
                if (this.splitMode[i, 1] > maxHeight) {
                    continue;
                }

                if (!IsValidSize(width, this.splitMode[i, 1])) {
                    continue;
                }

                int yRange = this.splitMode[i, 1] - this.splitMode[i, 0];
                if (!IsTransparent(frame, x, width, y + this.splitMode[i, 0], yRange)) {
                    height = this.splitMode[i, 1];
                }
            }

            minWidthConstraint++;
        }
    }

    private static bool IsValidSize(int width, int height)
    {
        if (width < 0 || width > 64 || width % 8 != 0) {
            return false;
        }

        if (height < 0 || height > 64 || height % 8 != 0) {
            return false;
        }

        if (width == 64 && (height == 8 || height == 16)) {
            return false;
        }

        if ((width == 8 || width == 16) && height == 64) {
            return false;
        }

        return true;
    }

    private static (int x, int y, FullImage trimmed) TrimImage(FullImage image)
    {
        // Get border points to get dimensions
        int xStart = SearchNoTransparentPoint(image, 1);
        int yStart = SearchNoTransparentPoint(image, 0);
        int width = SearchNoTransparentPoint(image, 2) - xStart + 1;
        int height = SearchNoTransparentPoint(image, 3) - yStart + 1;

        // Size must be multiple of 8 due to Obj size
        if (width % 8 != 0) {
            width += 8 - (width % 8);
        }

        if (height % 8 != 0) {
            height += 8 - (height % 8);
        }

        if (xStart == -1) {
            return (0, 0, image);
        }

        var newPixels = new Rgb[width * height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                newPixels[(y * width) + x] = image.Pixels[((yStart + y) * image.Width) + xStart + x];
            }
        }

        var newImage = new FullImage(width, height) {
            Pixels = newPixels,
        };

        return (xStart, yStart, newImage);
    }

    private static int SearchNoTransparentPoint(
        FullImage image,
        int direction,
        int xStart = 0,
        int yStart = 0,
        int xEnd = -1,
        int yEnd = -1)
    {
        if (xEnd == -1) {
            xEnd = image.Width;
        }

        if (yEnd == -1) {
            yEnd = image.Height;
        }

        int point = -1;
        Rgb[] pixels = image.Pixels;
        int width = image.Width;
        bool stop = false;

        // Get top most
        if (direction == 0) {
            for (int y = yStart; y < yEnd && !stop; y++) {
                for (int x = xStart; x < xEnd && !stop; x++) {
                    if (pixels[(y * width) + x].Alpha == 0) {
                        continue;
                    }

                    point = y;
                    stop = true;
                }
            }

            // Get left most
        } else if (direction == 1) {
            for (int x = xStart; x < xEnd && !stop; x++) {
                for (int y = yStart; y < yEnd && !stop; y++) {
                    if (pixels[(y * width) + x].Alpha == 0) {
                        continue;
                    }

                    point = x;
                    stop = true;
                }
            }

            // Get right most
        } else if (direction == 2) {
            for (int x = xEnd - 1; x > 0 && !stop; x--) {
                for (int y = yStart; y < yEnd && !stop; y++) {
                    if (pixels[(y * width) + x].Alpha == 0)
                        continue;

                    point = x;
                    stop = true;
                }
            }

            // Get bottom most
        } else if (direction == 3) {
            for (int y = yEnd - 1; y > 0 && !stop; y--) {
                for (int x = xStart; x < xEnd && !stop; x++) {
                    if (pixels[(y * width) + x].Alpha == 0)
                        continue;

                    point = y;
                    stop = true;
                }
            }
        } else {
            throw new ArgumentOutOfRangeException(nameof(direction), "Only 0 to 3 values");
        }

        return point;
    }

    private static bool IsTransparent(
        FullImage image,
        int xStart,
        int xRange,
        int yStart,
        int yRange)
    {
        bool isTransparent = true;
        int xEnd = xStart + xRange > image.Width ? image.Width : xStart + xRange;
        int yEnd = yStart + yRange > image.Height ? image.Height : yStart + yRange;

        var pixels = image.Pixels;
        bool stop = false;
        for (int x = xStart; x < xEnd && !stop; x++) {
            for (int y = yStart; y < yEnd && !stop; y++) {
                if (pixels[(y * image.Width) + x].Alpha != 0) {
                    isTransparent = false;
                    stop = true;
                }
            }
        }

        return isTransparent;
    }
}
