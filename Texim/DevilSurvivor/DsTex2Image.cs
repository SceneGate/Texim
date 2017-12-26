//
// DsTex2Image.cs
//
// Author:
//       Benito Palacios Sanchez <benito356@gmail.com>
//
// Copyright (c) 2017 Benito Palacios Sanchez
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace Texim.DevilSurvivor
{
    using System;
    using Yarhl.FileFormat;
    using Yarhl.IO;
    using Media.Image;

    public class DsTex2Image :
        IConverter<BinaryFormat, Image>,
        IConverter<Image, BinaryFormat>
    {
        public Image Convert(BinaryFormat source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            DataReader reader = new DataReader(source.Stream);

            Image image = new Image { Width = 128, Height = 128 };
            image.SetData(
                reader.ReadBytes((int)source.Stream.Length),
                PixelEncoding.Lineal,
                ColorFormat.Indexed_A3I5);
            
            return image;
        }

        public BinaryFormat Convert(Image source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            BinaryFormat binary = new BinaryFormat();
            DataWriter writer = new DataWriter(binary.Stream);

            writer.Write(source.GetData());
            return binary;
        }
    }
}
