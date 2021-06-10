# SceneGate [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://choosealicense.com/licenses/mit/) ![build](https://github.com/SceneGate/scenegate/workflows/Build%20and%20release/badge.svg)

**Work-in-progress** tool for reverse engineering, file format analysis, modding
and localization.

## Features

- Navigate the virtual file system from Yarhl.
- Convert files with Yarhl converters via plugins.
- View the content of files

### Supported format views

- .NET objects as property grid or YAML/JSON

### _Yarhly_ shipped plugins

- [Lemon](https://github.com/SceneGate/Lemon/)
- [Ekona](https://github.com/SceneGate/Ekona/)
- [Texim](https://github.com/SceneGate/Texim)
- [LayTea](https://github.com/pleonex/LayTea)

## Installation

Install the `Ubuntu Nerd Font` font from
[here](https://github.com/ryanoasis/nerd-fonts/releases/download/v2.1.0/Ubuntu.zip).

The project ships the application as a portable ZIP file that does not require
any installation. Just unzip and run!

<!-- prettier-ignore -->
| Release | Package |
| ------- | ------- |
| Stable  | [![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/SceneGate/scenegate?sort=semver)](https://github.com/SceneGate/SceneGate/releases) |
| Preview | [![GitHub commits since latest release (by SemVer)](https://img.shields.io/github/commits-since/SceneGate/scenegate/latest?sort=semver)](https://github.com/SceneGate/SceneGate/actions) |

## Documentation

Feel free to ask any question in the
[project Discussion site!](https://github.com/SceneGate/scenegate/discussions)

Check our on-line [documentation](https://scenegate.github.io/SceneGate/) for
more information about the file formats and how to use the tools.

## Build

The project requires to build .NET 5.0 SDK and .NET Core 3.1 runtime. If you
open the project with VS Code and you did install the
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

## License

The software is licensed under the terms of the
[MIT license](https://choosealicense.com/licenses/mit/). The information and
software provided by this repository is only for educational and research
purpose. Please support the original game developers by buying their games.

Icons:

- [Material design icons by Google](https://github.com/google/material-design-icons)
