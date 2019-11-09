This page describes how to build the project from its sources.

# Requirements

- MSBuild 2017 (included with Visual Studio 2017) is required to build the solution.
- Python3 is required to run build scripts, of which one generating metadata files for assembly information is integrated in the msbuild projects.

# How to build

The solution `Overmind.ImageManager.sln` can be opened and built with Visual Studio. The paths to the required executables are defined in `Environment.props` and can be overriden locally by creating a similar file `Environment.props.user`.

The solution can also be built using the python build scripts. The paths to the required executables are defined by `Scripts/environment.py` and can be overriden locally by creating a file `environment.json`, at the repository root or in your home directory.

```
python3 ./Scripts/main.py metadata
python3 ./Scripts/main.py compile --configuration Debug
```

The built executables can then be found in the `.build` directory, at the workspace root.

```
.build/WindowsClient/bin/Debug/Overmind.ImageManager.WindowsClient.exe
.build/WallpaperService/bin/Debug/Overmind.ImageManager.WallpaperService.exe
```

# How to build for release

When building the project for a new release, ensure your workspace has no local changes, then invoke the release command:

```
python3 ./Scripts/main.py release
```

The release can then be found as a zip archive in the `.artifacts` directory, at the workspace root.

```
.artifacts/packages/Overmind.ImageManager_$version-$revision_PackageFinal.zip
```
