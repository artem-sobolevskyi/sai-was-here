# SAI_WAS_HERE

GeForce NOW desktop toolkit for Windows sessions. It sets up a usable desktop shell, installs portable apps, configures Steam, and applies persistence helpers while a GFN session is running.

> Requires a real GeForce NOW environment (`C:\Asgard` must exist). Outside GFN the app exits immediately.

## Features

- Custom desktop shell and wallpaper
- Portable apps from a remote manifest (7-Zip, Notepad++, Explorer++, browsers, etc.)
- Silent helpers (Open-Shell, hotkeys, file associations)
- Steam proxy bypass / session helpers
- Optional .NET runtime install (5.0–10.0)
- Recovery mode (press **Del** on startup)

## Download

Public EXE (GitHub Release asset):

**https://github.com/artem-sobolevskyi/gfn-assets/releases/download/v1.6.8.1/SAI_WAS_HERE.exe**

Release page: https://github.com/artem-sobolevskyi/gfn-assets/releases/tag/v1.6.8.1

## Remote assets

Config JSON, wallpaper, and download manifests live in a separate public repo:

**https://github.com/artem-sobolevskyi/gfn-assets**

Install path on GFN (from `directory.json`):

```text
I:\Apps\SAI_WAS_HERE
```

## Build

Windows only. Needs Visual Studio / Build Tools with the **.NET desktop** workload (MSBuild).

```bat
publish.bat
```

Output:

```text
publish\SAI_WAS_HERE.exe
```

## Usage

1. Start a GeForce NOW session.
2. Run `SAI_WAS_HERE.exe`.
3. Optional: press **Del** within ~1.5s for recovery mode.
4. Optional custom apps JSON:

```bat
SAI_WAS_HERE.exe --apps-json path\to\apps.json
```

## Project layout

```text
SAI_WAS_HERE/          C# (.NET Framework 4.8) source
SAI_WAS_HERE.sln       Visual Studio solution
publish.bat            Release build script
wallpaper.jpeg         Default wallpaper source (also hosted in gfn-assets)
```

## License

MIT — see [LICENSE](LICENSE).
P.S Hack made by Dzoomyolo with love