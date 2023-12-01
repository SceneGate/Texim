# Texim [![awesomeness](https://img.shields.io/badge/SceneGate-awesome%20%F0%9F%95%B6-blue?logo=csharp)](https://github.com/SceneGate)

<!-- markdownlint-disable MD033 -->
<p align="center">
  <a href="https://github.com/SceneGate/Texim/workflows/Build and release">
    <img alt="Build and release" src="https://github.com/SceneGate/Texim/workflows/Build and release/badge.svg?branch=main" />
  </a>
  &nbsp;
  <a href="https://choosealicense.com/licenses/mit/">
    <img alt="MIT License" src="https://img.shields.io/badge/license-MIT-blue.svg?style=flat" />
  </a>
  &nbsp;
</p>

Extensible API for image-related formats. It aims to provide an API to easily
implement different image, palette, sprites and animation formats. Including
typical image processing algorithms for importers or hardware swizzlers of
different platforms.

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

## Documentation

Feel free to ask any question in the
[project Discussion site!](https://github.com/SceneGate/Texim/discussions)

Check our on-line [documentation](https://scenegate.github.io/Texim/).

## Usage

The project has the following .NET libraries (NuGet packages via nuget.org). The
libraries only support .NET LTS versions: **.NET 6.0** and **.NET 8.0**.

- `Texim`: Image formats and converters API
- `Texim.Formats.ImageMagick`: convert to ImageMagick supported formats.
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

## Contributing

The repository requires to build .NET 8.0 SDK.

To build, test and generate artifacts run:

```sh
# Build and run tests
dotnet run --project build/orchestrator

# (Optional) Create bundles (nuget, zips, docs)
dotnet run --project build/orchestrator -- --target=Bundle
```

Additionally you can use _Visual Studio_ or _JetBrains Rider_ as any other .NET
project.

To contribute follow the [contributing guidelines](CONTRIBUTING.md).

### How to release

Create a new _GitHub release_ with a tag name `v{Version}` (e.g. `v2.4`) and
that's it! This triggers a pipeline that builds and deploy the project.

## License

The software is licensed under the terms of the
[MIT license](https://choosealicense.com/licenses/mit/).
