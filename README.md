# Texim [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://choosealicense.com/licenses/mit/) ![Build and release](https://github.com/SceneGate/Texim/workflows/Build%20and%20release/badge.svg)

> Extensible API for image-related formats. It aims to provide an API to easily
> implement different image, palette, sprites and animation formats. Including
> typical image processing algorithms for importers or hardware swizzlers of
> different platforms.

**This is a proof-of-concept project for fast prototyping.** In the future it
will be _Yarhl.Media.Images_. There aren't stable releases, only preview. The
API may suffer major refactors between minor versions.

<!-- prettier-ignore -->
| Release | Package |
| ------- | ------- |
| Stable  | None    |
| Preview | [Azure Artifacts](https://dev.azure.com/SceneGate/SceneGate/_packaging?_a=feed&feed=SceneGate-Preview) |

## Documentation

Feel free to ask any question in the
[project Discussion site!](https://github.com/SceneGate/Texim/discussions)

Check our on-line [documentation](https://scenegate.github.io/Texim/).

## Build

The project requires to build .NET 6 SDK and .NET Framework or Mono. If you open
the project with VS Code and you did install the
[VS Code Remote Containers](https://code.visualstudio.com/docs/remote/containers)
extension, you can have an already pre-configured development environment with
Docker or Podman.

To build, test and generate artifacts run:

```sh
# Only required the first time
dotnet tool restore

# Default target is Stage-Artifacts
dotnet cake
```

To just build and test quickly, run:

```sh
dotnet cake --target=BuildTest
```
