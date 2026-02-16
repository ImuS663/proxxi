# proxxi

**proxxi** is a .NET toolchain for **fetching proxies from plugins** and **serializing them into multiple output formats**.

## Features

- Plugin-based proxy sources (load providers via plugins)
- Configurable data directory (supports environment-based configuration)
- Multiple output formats:
    - Plain text
    - URL list
    - JSON / JSONL
    - CSV / TSV / PSV
    - XML

## Requirements

- **.NET SDK 10.0+** (for building)

## Installation

### Option 1: Download a release attachment

Go to the GitHub Releases page and download the binary for your OS/architecture (e.g. `linux-x64`, `win-x64`, etc.).

### Option 2: Build from source

```shell
git clone https://github.com/proxxi/proxxi.git
cd proxxi
dotnet restore
dotnet build -c Release --self-contained true -p:DebugType=embedded -o publish
```

## CLI usage

> The CLI provides a `fetch` command that loads configured plugins, fetches proxies, and writes them in the chosen format.

### Run

```shell
./proxxi --help
```

### Fetch proxies (example)

```shell
./proxxi fetch publisher.plugin --output proxies.csv
```

## Configuration

### Proxxi data directory

Proxxi supports configuring its working directory via an environment variable.

```shell
export PROXXI_DIR="<ABSOLUTE_OR_RELATIVE_PATH>"
```

(If your shell is PowerShell on Windows, use `$env:PROXXI_DIR="..."`.)

> Proxxi will initialize required directories/files on the first run.

### Plugin configuration

You can manage plugins using the `plugin` command and its subcommands:

- `plugin <PLUGIN-ID> info` - Show information about a plugin
- `plugin <PLUGIN-ID> alias [ALIAS]` - Set or remove an alias for a plugin
- `plugin <PLUGIN-ID> enable` - Enable a plugin
- `plugin <PLUGIN-ID> disable` - Disable a plugin
- `plugin <PLUGIN-ID> parameter <NAME> [VALUE]` - Set or remove a parameter for a plugin
- `parameters` - List parameters for a plugin

You can also use the `plugins` command to list all installed plugins.

## Output formats

Proxxi can write proxies in several formats depending on command options:

- `plain` (one proxy per line)
- `url` (URL-style entries)
- `json`, `jsonl`
- `csv`, `tsv`, `psv`
- `xml`

## Plugins

### Creating plugins

To create Proxxi plugins, use the official plugin SDK as the base:

- **Proxxi Plugin SDK:** https://github.com/ImuS663/proxxi.plugin.sdk

### Plugins overview

A plugin exposes one or more proxy sources, and the CLI orchestrates:

1. locating/loading plugins
2. validating parameters
3. fetching proxies
4. writing output using the selected writer

### Adding plugins

1. **Choose your Proxxi directory** (optional but recommended for predictable paths):
    ```shell
    export PROXXI_DIR="<PATH_TO_PROXXI_DIR>"
    ```
2. **Run proxxi** for initial `PROXXI_DIR` and subfolders and files (if is first run):
    ```shell
    ./proxxi --help
    ```
3. **Copy the plugin** into the `plugins` directory. Typically, this is the plugin `.dll` plus any required dependency
   files that ship with it (keep every plugin in its own folder for simplicity and conflict prevention):
    ```shell
    cp -r <PLUGIN> "$PROXXI_DIR/plugins/"
    ```
4. **Register/enable the plugin** in the Proxxi plugin configuration (JSON) under the Proxxi directory.
    ```json5
    [
      {
        "id": "publisher.plugin",
        "path": "path/to/plugin/dll", // relative path from `$PROXXI_DIR/plugins/`
        "alias": "publisher", // optional
        "version": "1.0.0",
        "enabled": true,
        "params": {
          // plugin parameters if needed
          "param1": "value1",
          "param2": "value2"
        }
      }
    ]
    ```

> Tip: If a plugin is not discovered, ensure the plugin and its dependencies are present in the plugins folder and that the plugin is added/enabled in the JSON config.

## License

This project is licensed under the GNU General Public License v3.0 – see the [LICENSE](LICENSE) file for details.