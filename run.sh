#!/bin/bash

set -e

if [ -z "$1" ]; then
    filename="data/example.pqd"
else
    filename="$1"
fi

echo "Building the application"
echo "========================"
dotnet build --configuration release PQDIFConverter.sln

echo
echo Running the application on ${filename}
echo ===========================
rm -f pqdif_temp.db*
rm -f output_*.csv
rm -rf output
dotnet run --configuration release --project pqdif-converter ${filename} $2
