# VDFparse

Convert binary valve data files (`.vdf` files) into json.

## Usage
`vdfparse [-i/--info] file [id [id ...]]`

`file` can be a path to a vdf file or `appinfo`/`packageinfo`
in which case it will try to locate these files through
querying the registry for the steam location (Windows)
or trying some common locations (Linux).

## Building
TODO
