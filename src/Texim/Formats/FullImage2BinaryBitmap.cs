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
namespace Texim.Formats
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using Texim.Images;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public class FullImage2BinaryBitmap :
        IInitializer<ImageFormat>, IConverter<IFullImage, BinaryFormat>
    {
        private ImageFormat format = ImageFormat.Png;

        public void Initialize(ImageFormat parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            format = parameters;
        }

        public BinaryFormat Convert(IFullImage source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var image = new Bitmap(source.Width, source.Height);
            for (int x = 0; x < source.Width; x++) {
                for (int y = 0; y < source.Height; y++) {
                    image.SetPixel(x, y, source.Pixels[y * source.Width + x].ToColor());
                }
            }

            var binary = new BinaryFormat();
            image.Save(binary.Stream, format);
            return binary;
        }
    }
}
