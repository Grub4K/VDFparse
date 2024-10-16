# VDFparse
Convert known binary valve data files (`.vdf` files) into json.

## Usage
```
Description:
  Convert known binary valve data files (`.vdf` files) into json.

Usage:
  VDFparse <path> [<id>...] [options]

Arguments:
  <path>  The path to the vdf file, `appinfo`/`packageinfo` to search or `-` to read from stdin.
  <id>    Ids to filter by. If no id is specified output all ids.

Options:
  -i, --info-only        Show only info and omit main data. [default: False]
  -p, --pretty           Indent the JSON output. [default: False]
  -o, --output <output>  The output path or `-` to write to stdout [default: -]
  --version              Show version information
  -?, -h, --help         Show help and usage information
```
If `path` is `appinfo`/`packageinfo` it will try to locate them
by querying the registry for the steam location (Windows)
or trying some common locations (Linux).

## Building
Building can simply be done using `dotnet build`/`dotnet publish`.
More information can be found [here](https://learn.microsoft.com/dotnet/core/tools/dotnet-build) and [here](https://learn.microsoft.com/dotnet/core/tools/dotnet-publish).
