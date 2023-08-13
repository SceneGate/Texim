namespace Texim.Processing;

using Texim.Palettes;
using Texim.Pixels;

public class QuantizationResult
{
    public IndexedPixel[] Pixels { get; init; }

    public IPaletteCollection Palettes { get; init; }
}
