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
namespace Texim.Games.Nitro;

using Texim.Sprites;

/// <summary>
/// Nitro sprite image segment format: Object attribute memory (OAM).
/// </summary>
public class ObjectAttributeMemory : IImageSegment
{
    public ObjectAttributeMemory()
    {
    }

    public ObjectAttributeMemory(IImageSegment other)
    {
        Layer = other.Layer;
        CoordinateX = other.CoordinateX;
        CoordinateY = other.CoordinateY;
        Width = other.Width;
        Height = other.Height;
        TileIndex = other.TileIndex;
        HorizontalFlip = other.HorizontalFlip;
        VerticalFlip = other.VerticalFlip;
        PaletteIndex = other.PaletteIndex;
        HasRotationOrScaling = false;
        HasDoubleSize = false;
        IsDisabled = false;
        Mode = ObjectAttributeMemoryMode.Normal;
        IsMosaic = false;
        PaletteMode = NitroPaletteMode.Palette16x16;
    }

    public int Layer { get; set; }

    public int CoordinateX { get; set; }

    public int CoordinateY { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public int TileIndex { get; set; }

    public bool HorizontalFlip { get; set; }

    public bool VerticalFlip { get; set; }

    public byte PaletteIndex { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether supports rotation or scaling.
    /// </summary>
    public bool HasRotationOrScaling { get; set; }

    /// <summary>
    /// Gets or sets the rotation or scaling group number.
    /// There are up to 32 different groups.
    /// </summary>
    /// <description>
    /// The rotation / scaling groups are located after the OAM value in the RAM.
    /// <see href="http://nocash.emubase.de/gbatek.htm#lcdobjoamrotationscalingparameters"/>
    /// The transformations are the same as for BG images:
    /// <see href="http://nocash.emubase.de/gbatek.htm#lcdiobgrotationscaling"/>
    /// In general, given a group of 4 parameter: A, B, C and D the transformation point is:
    ///   x2 = A*(x1-x0) + B*(y1-y0) + x0
    ///   y2 = C*(x1-x0) + D*(y1-y0) + y0
    /// where (x0, y0) is the rotation center, (x1, y1) is the old point and (x2, y2) the new point.
    /// </description>
    public byte RotationOrScalingGroup { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether supports double size.
    /// </summary>
    /// <description>
    /// The sprites are displayed inside a rectangular area. When the sprite is rotated or scaled this area
    /// could be smaller than needed and some parts could be not displayed.
    /// Enabling this feature, the rectangular area will be multiplied by 2.
    /// </description>
    /// <remarks>Only if Rotation/Scaling is enabled.</remarks>
    public bool HasDoubleSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether is disabled.
    /// </summary>
    /// <remarks>Only if Rotation/Scaling is disabled.</remarks>
    public bool IsDisabled { get; set; }

    public ObjectAttributeMemoryMode Mode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether mosaic mode is enabled.
    /// </summary>
    /// <see href="http://nocash.emubase.de/gbatek.htm#lcdiomosaicfunction"/>
    public bool IsMosaic { get; set; }

    public NitroPaletteMode PaletteMode { get; set; }
}
