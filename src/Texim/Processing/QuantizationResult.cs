namespace Texim.Processing;

using Palettes;
using Pixels;

public class QuantizationResult
{
    public IndexedPixel[] Pixels { get; init; }

    public IPaletteCollection Palettes { get; init; }
}
