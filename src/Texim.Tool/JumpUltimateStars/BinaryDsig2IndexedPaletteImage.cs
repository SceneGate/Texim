using System;
using Texim.Colors;
using Texim.Images;
using Texim.Palettes;
using Texim.Pixels;
using Yarhl.FileFormat;
using Yarhl.IO;

namespace Texim.Tool.JumpUltimateStars
{
    public class BinaryDsig2IndexedPaletteImage : IConverter<IBinary, IndexedPaletteImage>
    {
        public IndexedPaletteImage Convert(IBinary source)
        {
            if (source is null) {
                throw new ArgumentNullException(nameof(source));
            }

            var reader = new DataReader(source.Stream);
            source.Stream.Position = 0;

            if (reader.ReadString(4) != "DSIG") {
                throw new FormatException("Invalid stamp");
            }

            reader.ReadByte();
            bool is8Bpp = reader.ReadByte() == 0x10;
            short numPalettes = reader.ReadInt16();
            int width = reader.ReadUInt16();
            int height = reader.ReadUInt16();

            var palettes = new PaletteCollection();
            int colorsPerPalette = is8Bpp ? 256 : 16;
            for (int i = 0; i < numPalettes; i++) {
                palettes.Palettes.Add(new Palette(reader.ReadColors<Bgr555>(colorsPerPalette)));
            }

            IIndexedPixelEncoding pixelEncoding = is8Bpp ? Indexed8Bpp.Instance : Indexed4Bpp.Instance;
            var pixels = pixelEncoding.Decode(source.Stream, width * height);

            var image = new IndexedPaletteImage {
                Width = width,
                Height = height,
                Pixels = pixels,
            };
            image.Palettes.Add(palettes.Palettes);

            return image;
        }
    }
}
