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
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.Formats.Png;
    using Texim.Palettes;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;

    public class PaletteCollection2ContainerBitmap : IConverter<IPaletteCollection, NodeContainerFormat>
    {
        private readonly IImageEncoder encoder;

        public PaletteCollection2ContainerBitmap()
        {
            encoder = new PngEncoder();
        }

        public PaletteCollection2ContainerBitmap(IImageEncoder parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);
            encoder = parameters;
        }

        public NodeContainerFormat Convert(IPaletteCollection source)
        {
            ArgumentNullException.ThrowIfNull(source);

            var container = new NodeContainerFormat();
            for (int i = 0; i < source.Palettes.Count; i++) {
                var child = new Node($"Palette {i}", source.Palettes[i])
                    .TransformWith(new Palette2Bitmap(encoder));
                container.Root.Add(child);
            }

            return container;
        }
    }
}
