# Texim ![MIT license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)

Extensible API for image-related formats in the
[Yarhl](https://scenegate.github.io/Yarhl/) framework. It aims to provide an API
to easily implement different image, palette, sprites and animation formats.
Including typical image processing algorithms for importers or hardware
swizzlers of different platforms.

> [!NOTE]  
> **This is a proof-of-concept project for fast prototyping.** In the future it
> will be _Yarhl.Media.Images_. There aren't stable releases, only preview. The
> API may suffer major refactors between minor versions.

## Features

- 🔴🟢🔵 **Colors**
  - Serialize and deserialize data as RGB32 or BGR555 colors
  - Indexed pixels with different bit-depths: 4LE, 4BE, 8
  - Alpha channels RGBA32 and indexed formats: A3I5 and A5I3.
  - Extensible pixel encoding interfaces.
- 🎨 **Palettes**
  - Palette format interfaces
  - Convert to an image color map
  - Convert to `RIFF` format.
  - Convert from raw data.
- 🖼️ **Images**
  - Interfaces for RGB and indexed images
  - Converters to/from RGB and indexed images
  - Image quantization algorithms for indexed images
    - Standard quantization
    - Fixed palette
    - Fixed palette per tile
    - Median cut
    - Exhaustive color search algorithm
  - Animated images
  - Convert from raw data
  - Supports tile-based images / swizzling algorithms
    - NDS map compression
- 🧩 **Sprites** (segmented layered images)
  - Extensible interfaces
  - Support for segments and layers
  - Export into layered TIFF images
  - Import from multi-page TIFF images
  - Segmentation algorithms
    - NDS segmentation
    - TIFF multi-pages
- 📃 **Standard image formats** conversion
  - PNG, BMP, JPG and anything supported ImageSharp
  - TIFF with multi-page support
  - GIF
  - Convert to formats supported by ImageMagick
- 🎮 **Game formats**
  - Nitro formats: `NCLR`, `NCGR`, `NSCR`, `NCER`
  - Partial support from games:
    - _Black Rock Shooter_
    - _Devil Survivor_
    - _Disgaea_
    - _Jump Ultimate Stars_
    - _London Life_
    - _Megaman_
    - _MetalMax_
  - Raw binary formats

## Usage

The project has the following .NET libraries (NuGet packages via nuget.org). The
libraries only support .NET LTS versions: **.NET 6.0** and **.NET 8.0**.

- `Texim`: Image formats and converters API
- `Texim.Games`: Format and converter implementations from games.

The libraries are available from the Azure DevOps public feeds. To use them
create a file `nuget.config` in the same directory of your solution file (.sln)
with the following content:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear/>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="SceneGate-Preview" value="https://pkgs.dev.azure.com/SceneGate/SceneGate/_packaging/SceneGate-Preview/nuget/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="SceneGate-Preview">
      <package pattern="Texim*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

Then restore / install as usual via Visual Studio, Rider or command-line. You
may need to restart Visual Studio for the changes to apply.
