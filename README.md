# PQDIF to CSV converter

A simple command line program that extracts all the data from a PQDIF file and writes it to a collection of CSV files.

Uses the [Gemstone.PQDIF](https://github.com/Gemstone/PQDIF) library to parse the PQDIF file.

## Compiling the program

- Requirements:
    - .NET SDK 9.0 ([https://learn.microsoft.com/en-us/dotnet/core/install/](https://learn.microsoft.com/en-us/dotnet/core/install/))

- To build a self-contained application:
    ```bash
    ./build.sh <TARGET_ARCHITECTURE>
    ```

    where `<TARGET_ARCHITECTURE>` corresponds to the operating system you are building for, i.e. an RID listed at [https://learn.microsoft.com/en-us/dotnet/core/rid-catalog](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog). The following RIDs are the most common:

    - Windows: `win-x64` or `win-arm64`
    - Linux: `linux-x64` or `linux-arm64`
    - macOS: `osx-x64` or `osx-arm64`

    The executable will be written to folder `./bin/<TARGET_ARCHITECTURE>` along with an XML file containing the PQDIF identifiers.

## Running the program

The compiled application contains all necessary dependencies and can be executed directly:

```bash
./pqdif-converter <PATH_TO_PQDIF_FILE> <PATH_TO_OUTPUT_DIRECTORY>
```

where `<PATH_TO_PQDIF_FILE>` is the path to the PQDIF file to convert, and `<PATH_TO_OUTPUT_DIRECTORY>` is the path to the directory to write the CSV files to. The default output directory is `./output`.

Alternatively the compilation and the execution can be done in a single step (requires .NET SDK 9.0):

```bash
./run.sh <PATH_TO_PQDIF_FILE> <PATH_TO_OUTPUT_DIRECTORY>
```
