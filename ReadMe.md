# Overmind Image Manager

Overmind Image Manager is a Windows desktop application to manage your collections of pictures. 


## Build

- MSBuild 2017 (or simply Visual Studio 2017) is required to build the solution.
- Python3 is required to run build scripts, of which one generating metadata files for assembly information is integrated in the msbuild projects.

The solution `Overmind.ImageManager.sln` can be opened and built directly with Visual Studio. The paths to the required executables are defined in `Environment.props` and can be overriden locally by creating a similar file `Environment.props.user`.

The solution can also be built directly using the python build scripts. The paths to the required executables are defined by `Scripts/environment.py` and can be overriden locally by creating a file `environment.json`, at the repository root or in a directory `.overmind` in your home directory.

```
python3 ./Scripts/main.py metadata
python3 ./Scripts/main.py compile --configuration Debug
```
