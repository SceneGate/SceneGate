# ![logo](docs/images/logo_48.png) SceneGate

<!-- markdownlint-disable MD033 -->
<p align="center">
  <a href="https://github.com/SceneGate/SceneGate/releases">
    <img alt="Stable version" src="https://img.shields.io/github/v/release/SceneGate/scenegate?sort=semver" />
  </a>
  &nbsp;
  <a href="https://github.com/SceneGate/SceneGate/actions">
    <img alt="GitHub commits since latest release (by SemVer)" src="https://img.shields.io/github/commits-since/SceneGate/scenegate/latest?sort=semver" />
  </a>
  &nbsp;
  <a href="https://github.com/SceneGate/scenegate/workflows/Build%20and%20release">
    <img alt="Build and release" src="https://github.com/SceneGate/scenegate/workflows/Build%20and%20release/badge.svg" />
  </a>
  &nbsp;
  <a href="https://choosealicense.com/licenses/mit/">
    <img alt="MIT License" src="https://img.shields.io/badge/license-MIT-blue.svg?style=flat" />
  </a>
  &nbsp;
</p>

> [!CAUTION]  
> **This project is still in a very early development phase (pre-alpha).** Until we do a first release, there will be **no downloads available**.
> The current version **crashes frequently** and lacks many usability features for non-developer users.
> 
> If you are a Tinke user, be aware this project is not a replacement yet. It has a long way to go until it has the same feature set.
> 
> For now, this application is **only useful if you are a .NET/C# developer**, you are already familiar with [Yarhl](https://github.com/sceneGate/yarhl) and you want to test your converters.
> The best way to get the latest dev version of the app, it's to [build it locally](#Build). **Creating a fork and running the CI may not work.**

**Work-in-progress** tool for reverse engineering, file format analysis, modding
and localization.

- 📁 Navigate the virtual file system from Yarhl.
- 🔁 Convert files with Yarhl converters via plugins.
- 🔎 View the content of files
  - .NET objects as property grid or YAML/JSON
  - PO viewer and editor

![Demo PO format view](./docs/images/demo-poview.png)

![Demo object view](./docs/images/demo-objview.png)

## Supported plugins

- 🔧 Generic:
  - [Yarhl.Media.Text](https://scenegate.github.io/Yarhl/docs/media-text/po-format.html):
    PO translation
  - [Texim](https://github.com/SceneGate/Texim): standard images
- 🕹️ Platforms:
  - [Ekona](https://scenegate.github.io/Ekona/): DS and DSi ROM
  - [Lemon](https://scenegate.github.io/Lemon/): 3DS ROM
  - [Texim.Games](https://github.com/SceneGate/Texim): DS images
- 🎩 Games:
  - [Texim.Games](https://github.com/SceneGate/Texim): images from some DS games
  - [LayTea](https://www.pleonex.dev/LayTea/): Professor Layton games (London
    Life only for now)
  - AmbitionConquest: Pokémon Conquest DS

## Installation

The project ships the application as a portable ZIP file that does not require
any installation. Just unzip and run!

## Documentation

Feel free to ask any question in the
[project Discussion site!](https://github.com/SceneGate/scenegate/discussions)

Check our on-line [documentation](https://scenegate.github.io/SceneGate/) for
more information about the file formats and how to use the tools.

## Build

The project requires to build .NET 8.0 SDK.

To build, test and generate artifacts run:

```sh
# Build and run tests
dotnet run --project build/orchestrator

# (Optional) Create bundles (nuget, zips, docs)
dotnet run --project build/orchestrator -- --target=Bundle
```

## License

The software is licensed under the terms of the
[MIT license](https://choosealicense.com/licenses/mit/).

The information and software provided by this repository is only for educational
and research purpose. Please support the original game developers by buying
their games.
