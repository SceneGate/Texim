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
namespace Texim.Compressions.Nitro
{
    using System;
    using Texim.Images;
    using Texim.Processing;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;

    public class FullImageMapCompression : IConverter<IFullImage, NodeContainerFormat>
    {
        private readonly FullImageMapCompressionParams parameters;

        public FullImageMapCompression(FullImageMapCompressionParams parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);

            this.parameters = parameters;
        }

        public NodeContainerFormat Convert(IFullImage source)
        {
            ArgumentNullException.ThrowIfNull(source);

            // TODO: Avoid several swizzling by providing an argument to quantization
            // and compression (in and out, out here too).
            var quantization = new FixedPaletteTileQuantization(
                parameters.Palettes,
                parameters.TileSize,
                source.Width);
            var result = quantization.Quantize(source.Pixels);

            var fullIndexedImage = new IndexedImage(source.Width, source.Height, result.Pixels);

            var compression = new MapCompression(parameters);
            return compression.Convert(fullIndexedImage);
        }
    }
}
