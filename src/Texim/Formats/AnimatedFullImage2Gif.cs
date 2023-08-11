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
namespace Texim.Formats;

using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Texim.Animations;
using Texim.Images;
using Yarhl.FileFormat;
using Yarhl.IO;

public class AnimatedFullImage2Gif : IConverter<IAnimatedFullImage, BinaryFormat>
{
    public BinaryFormat Convert(IAnimatedFullImage source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (source.Frames.Count == 0)
            throw new FormatException("Missing frames");

        var firstFrame = source.Frames[0];
        int width = firstFrame.Image.Width;
        int height = firstFrame.Image.Height;
        var gif = new Image<Rgba32>(width, height);

        var metadata = gif.Metadata.GetGifMetadata();
        metadata.RepeatCount = (ushort)source.Loops;

        foreach (var frame in source.Frames) {
            if (frame.Image.Width != width || frame.Image.Height != height) {
                throw new FormatException("Every frame must have the same dimension");
            }

            var image = ConvertToImageSharp(frame.Image).Frames.RootFrame;
            var frameMetadata = image.Metadata.GetGifMetadata();
            frameMetadata.FrameDelay = frame.Duration;

            _ = gif.Frames.AddFrame(image);
        }

        var binary = new BinaryFormat();
        gif.SaveAsGif(binary.Stream);
        return binary;
    }

    private static Image<Rgba32> ConvertToImageSharp(IFullImage source)
    {
        var image = new Image<Rgba32>(source.Width, source.Height);
        for (int x = 0; x < source.Width; x++) {
            for (int y = 0; y < source.Height; y++) {
                image[x, y] = source.Pixels[(y * source.Width) + x].ToImageSharpColor();
            }
        }

        return image;
    }
}
