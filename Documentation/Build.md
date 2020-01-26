This page describes how to build the project from its sources.

# Requirements

- MSBuild 2019 (included with Visual Studio 2019) is required to build the solution.
- Python3 is required to run build scripts, of which one generating metadata files for assembly information is integrated in the msbuild projects.

# Development

The solution `Overmind.ImageManager.sln` can be opened and built with Visual Studio. The paths to the required executables are defined in `Environment.props` and can be overriden locally by creating a similar file `Environment.props.user`.

Additionally, the project include commands to automate development related tasks. They are exposed by the `development/main.py` script. Check the script help, using the `--help` option, for information about commands. You can also run commands with the `--simulate` option to check their behavior before actually running them.

To set up a workspace for development, create a `python3` virtual environment, then run the `develop` command. This will install the project dependencies in your python environment.

```
python ./development/main.py develop
```

The paths to required executables are defined by `development/environment.py` and can be overriden locally by creating a file `environment.json`, at the repository root or in your home directory.

To build the project, run the metadata and compile commands:

```
python ./development/main.py metadata
python ./development/main.py compile --configuration Debug
```

The built executables can then be found in the `.build` directory, at the workspace root.

```
.build/WindowsClient/Binaries/Debug/Overmind.ImageManager.WindowsClient.exe
.build/WallpaperService/Binaries/Debug/Overmind.ImageManager.WallpaperService.exe
```

# Release

When building the project for a new release, ensure your workspace has no local changes, then invoke the release command:

```
python ./development/main.py release
```

The release can then be found as a zip archive in the `.artifacts` directory, at the workspace root.

```
.artifacts/packages/Overmind.ImageManager_$version-$revision_PackageFinal.zip
```
